#nullable enable
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
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.File;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, DeleteFileCommandResult>
{
    private readonly IMediator _bus;
    private readonly ILogger<DeleteFileCommandHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IMediaRepository _mediaRepository;
    private readonly IFilesStorageConfigurationService _filesStorageConfigService;

    public DeleteFileCommandHandler(
        IMediator bus,
        ILogger<DeleteFileCommandHandler> logger,
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

    public async Task<DeleteFileCommandResult> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando exclusão de arquivo. Id: {Id}", request.Id);

        // Validar dados do usuário logado
        if (request.UserData == null || !request.UserData.IsValid())
        {
            _logger.LogWarning("UserData inválido ou não fornecido");
            await _bus.Publish(new ExceptionNotification("065", "Não foi possível identificar o usuário. Token inválido.", ExceptionType.Client, "UserData"), cancellationToken);
            return null!;
        }

        // Validar Id
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            _logger.LogWarning("Id não fornecido");
            await _bus.Publish(new ExceptionNotification("073", "Id é obrigatório", ExceptionType.Client, "Id"), cancellationToken);
            return null!;
        }

        // Buscar o arquivo no MongoDB pelo Id
        var media = await _mediaRepository.GetByIdAsync(request.Id, cancellationToken);
        if (media == null)
        {
            _logger.LogWarning("Arquivo não encontrado no MongoDB. Id: {Id}", request.Id);
            await _bus.Publish(new ExceptionNotification("074", "Arquivo não encontrado", ExceptionType.Client, "Id"), cancellationToken);
            return null!;
        }

        // Determinar o nome do arquivo salvo (uuid.ext)
        var fileNameUrl = media.FileNameUrl;
        if (string.IsNullOrWhiteSpace(fileNameUrl) && !string.IsNullOrWhiteSpace(media.Url))
        {
            // fallback: extrair do final da URL completa
            try
            {
                fileNameUrl = new Uri(media.Url).Segments[^1];
            }
            catch
            {
                // se não for URL válida, tenta pegar após a última barra
                var idx = media.Url.LastIndexOf("/", StringComparison.Ordinal);
                fileNameUrl = idx >= 0 ? media.Url[(idx + 1)..] : media.Url;
            }
        }

        // Buscar configurações do S3
        var s3Config = await _filesStorageConfigService.GetConfigurationAsync(cancellationToken);

        // Se S3 estiver habilitado, deletar do S3
        if (s3Config.Status && !string.IsNullOrWhiteSpace(s3Config.Bucket) && !string.IsNullOrWhiteSpace(fileNameUrl))
        {
            _logger.LogInformation("Deletando arquivo do S3. Bucket: {Bucket}, StageId: {StageId}, FileName: {FileName}", 
                s3Config.Bucket, media.StageId, fileNameUrl);
            
            try
            {
                await DeleteFromS3Async(s3Config, media.StageId, fileNameUrl, cancellationToken);
                _logger.LogInformation("Arquivo deletado do S3 com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao deletar arquivo do S3. Continuando com a exclusão do MongoDB.");
                // Continuar mesmo se houver erro ao deletar do S3
            }
        }

        // Buscar arquivo físico recursivamente (para deletar local se existir)
        var uploadsPath = _configuration["FileUpload:Path"] ?? "uploads";
        var basePath = Path.Combine(_environment.ContentRootPath, uploadsPath);
        
        string? filePath = null;
        if (Directory.Exists(basePath))
        {
            if (!string.IsNullOrWhiteSpace(fileNameUrl))
            {
                var files = Directory.GetFiles(basePath, fileNameUrl, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    filePath = files[0];
                }
            }
        }

        // Deletar arquivo físico se existir
        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
                _logger.LogInformation("Arquivo físico deletado com sucesso. Path: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao deletar arquivo físico. Path: {FilePath}. Continuando com a exclusão do MongoDB.", filePath);
                // Continuar mesmo se houver erro ao deletar o arquivo físico
            }
        }
        else
        {
            _logger.LogWarning("Arquivo físico não encontrado. fileNameUrl: {FileNameUrl}", fileNameUrl ?? "null");
        }

        // Deletar do MongoDB
        try
        {
            await _mediaRepository.DeleteAsync(media.Id, cancellationToken);
            _logger.LogInformation("Media deletado do MongoDB com sucesso. Id: {MediaId}", media.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar Media do MongoDB. Id: {MediaId}", media.Id);
            await _bus.Publish(new ExceptionNotification("075", $"Erro ao deletar arquivo do banco de dados: {ex.Message}", ExceptionType.Server, "Delete"), cancellationToken);
            return null!;
        }

        return new DeleteFileCommandResult
        {
            Success = true,
            Message = "Arquivo deletado com sucesso",
            Id = request.Id,
            FileNameUrl = fileNameUrl
        };
    }

    private async Task DeleteFromS3Async(
        FilesStorageConfiguration s3Config,
        string stageId,
        string fileName,
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

        // Deletar arquivo do S3
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = s3Config.Bucket,
            Key = s3Key
        };

        await s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
    }

    private string GetS3Key(string folder, string stageId, string fileName)
    {
        // Construir chave: folder/stageId/fileName
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
