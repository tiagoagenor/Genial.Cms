using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.File;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadFileCommandResult>
{
    private readonly IMediator _bus;
    private readonly ILogger<UploadFileCommandHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IMediaRepository _mediaRepository;
    private readonly IFilesStorageConfigurationService _filesStorageConfigService;

    public UploadFileCommandHandler(
        IMediator bus,
        ILogger<UploadFileCommandHandler> logger,
        IHostEnvironment environment,
        IConfiguration configuration,
        IMediaRepository mediaRepository,
        IFilesStorageConfigurationService filesStorageConfigService)
    {
        _bus = bus;
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
        _mediaRepository = mediaRepository;
        _filesStorageConfigService = filesStorageConfigService;
    }

    public async Task<UploadFileCommandResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando upload de arquivo. Nome: {FileName}, Tamanho: {FileSize} bytes", 
            request.File.FileName, request.File.Length);

        // Validar dados do usuário logado
        if (request.UserData == null || !request.UserData.IsValid())
        {
            _logger.LogWarning("UserData inválido ou não fornecido");
            await _bus.Publish(new ExceptionNotification("065", "Não foi possível identificar o usuário. Token inválido.", ExceptionType.Client, "UserData"), cancellationToken);
            return null!;
        }

        // Validar arquivo
        if (request.File == null || request.File.Length == 0)
        {
            _logger.LogWarning("Arquivo não fornecido ou vazio");
            await _bus.Publish(new ExceptionNotification("070", "Arquivo não fornecido ou vazio", ExceptionType.Client, "File"), cancellationToken);
            return null!;
        }

        // Obter diretório de uploads da configuração ou usar padrão
        var uploadsPath = _configuration["FileUpload:Path"] ?? "uploads";
        var basePath = Path.Combine(_environment.ContentRootPath, uploadsPath);
        
        // Criar pasta do stage dentro de uploads
        var stagePath = Path.Combine(basePath, request.UserData.StageId);
        
        if (!Directory.Exists(stagePath))
        {
            Directory.CreateDirectory(stagePath);
            _logger.LogInformation("Diretório do stage criado: {Path}", stagePath);
        }

        // Gerar UUID único para o arquivo
        var fileId = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(request.File.FileName);
        
        // Nome do arquivo: UUID + extensão
        var fileName = $"{fileId}{fileExtension}";

        // Buscar configurações do S3
        var s3Config = await _filesStorageConfigService.GetConfigurationAsync(cancellationToken);
        
        string fullUrl = string.Empty;
        string filePath = string.Empty;
        bool uploadedToS3 = false;

        try
        {
            // Se S3 estiver habilitado, fazer upload no S3
            if (s3Config.Status && !string.IsNullOrWhiteSpace(s3Config.Bucket))
            {
                _logger.LogInformation("Fazendo upload no S3. Bucket: {Bucket}", s3Config.Bucket);
                
                await UploadToS3Async(request, fileName, s3Config, request.UserData.StageId, cancellationToken);
                filePath = $"s3://{s3Config.Bucket}/{GetS3Key(s3Config.Folder, request.UserData.StageId, fileName)}";
                uploadedToS3 = true;
                
                // Usar URL local mesmo quando upload é no S3
                var baseUrl = _configuration["FileUpload:BaseUrl"] ?? "http://localhost:5000/v1/files/upload";
                fullUrl = $"{baseUrl.TrimEnd('/')}/{request.UserData.StageId}/{fileName}";
                
                _logger.LogInformation("Arquivo enviado para S3 com sucesso. URL local: {Url}", fullUrl);
            }
            else
            {
                // Upload local (comportamento atual)
                _logger.LogInformation("Fazendo upload local (S3 desabilitado)");
                
                filePath = Path.Combine(stagePath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                _logger.LogInformation("Arquivo salvo localmente com sucesso. Path: {FilePath}, Tamanho: {FileSize} bytes", 
                    filePath, request.File.Length);

                // Obter base URL da configuração
                var baseUrl = _configuration["FileUpload:BaseUrl"] ?? "http://localhost:5000/v1/files/upload";
                // URL inclui stageId/fileName para acesso direto
                fullUrl = $"{baseUrl.TrimEnd('/')}/{request.UserData.StageId}/{fileName}";
            }

            // Criar entidade Media para salvar no MongoDB
            var media = new Media
            {
                FileName = request.File.FileName,
                FileNameUrl = fileName, // uuid + extensão (nome salvo no disco e usado na URL)
                ContentType = request.File.ContentType ?? "application/octet-stream",
                FileSize = request.File.Length,
                Url = fullUrl, // URL completa
                Tags = new List<string>(), // Tags podem ser adicionadas posteriormente
                Extension = fileExtension,
                StageId = request.UserData.StageId, // Stage do usuário logado
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Salvar no MongoDB
            await _mediaRepository.InsertAsync(media, cancellationToken);
            _logger.LogInformation("Media salvo no MongoDB. Id: {MediaId}, Url: {Url}, S3: {S3}", 
                media.Id, media.Url, uploadedToS3);

            return new UploadFileCommandResult
            {
                FileId = media.Id, // ID do MongoDB (ObjectId)
                FileName = request.File.FileName,
                FilePath = filePath,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType ?? "application/octet-stream",
                Url = fullUrl, // URL completa
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar arquivo. S3: {S3}, Path: {FilePath}", uploadedToS3, filePath);
            await _bus.Publish(new ExceptionNotification("071", $"Erro ao salvar arquivo: {ex.Message}", ExceptionType.Server, "File"), cancellationToken);
            return null!;
        }
    }

    private async Task UploadToS3Async(
        UploadFileCommand request, 
        string fileName, 
        FilesStorageConfiguration s3Config, 
        string stageId, 
        CancellationToken cancellationToken)
    {
        // Configurar cliente S3
        var awsConfig = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(s3Config.Region)
        };

        // Se tiver endpoint customizado (ex: MinIO, LocalStack), usar
        if (!string.IsNullOrWhiteSpace(s3Config.Endpoint))
        {
            awsConfig.ServiceURL = s3Config.Endpoint;
            awsConfig.ForcePathStyle = true; // Necessário para MinIO e compatíveis
        }

        using var s3Client = new AmazonS3Client(
            s3Config.AccessKeyId,
            s3Config.SecretAccessKey,
            awsConfig
        );

        // Construir chave S3: folder/stageId/fileName
        var s3Key = GetS3Key(s3Config.Folder, stageId, fileName);

        // Fazer upload do arquivo
        var putRequest = new PutObjectRequest
        {
            BucketName = s3Config.Bucket,
            Key = s3Key,
            InputStream = request.File.OpenReadStream(),
            ContentType = request.File.ContentType ?? "application/octet-stream"
        };

        await s3Client.PutObjectAsync(putRequest, cancellationToken);
    }

    private string GetS3Key(string folder, string stageId, string fileName)
    {
        // Construir chave: folder/stageId/fileName
        // Remover barras duplicadas e normalizar
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(folder))
        {
            parts.Add(folder.Trim('/'));
        }
        
        if (!string.IsNullOrWhiteSpace(stageId))
        {
            parts.Add(stageId.Trim('/'));
        }
        
        parts.Add(fileName);
        
        return string.Join("/", parts);
    }
}
