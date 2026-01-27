#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Application.Commands.TypeFile;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using Genial.Cms.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/files")]
[Authorize]
public class FilesController : BaseController
{
    private readonly IJwtUserService _jwtUserService;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IFilesStorageConfigurationService _filesStorageConfigService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus,
        IJwtUserService jwtUserService,
        IHostEnvironment environment,
        IConfiguration configuration,
        IFilesStorageConfigurationService filesStorageConfigService,
        ILogger<FilesController> logger)
        : base(notifications, bus)
    {
        _jwtUserService = jwtUserService;
        _environment = environment;
        _configuration = configuration;
        _filesStorageConfigService = filesStorageConfigService;
        _logger = logger;
    }

    [HttpGet("type")]
    [SwaggerOperation("Listar todos os tipos de arquivo")]
    [ProducesResponseType(typeof(Response<GetTypeFilesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTypeFilesAsync()
    {
        var command = new GetTypeFilesCommand();
        var response = await Bus.Send(command);

        if (response == null)
        {
            response = new GetTypeFilesResponse
            {
                Data = new List<TypeFileDto>()
            };
        }

        return CreateResponse(Ok(new Response<GetTypeFilesResponse>(response)));
    }

    [HttpPost("upload")]
    [SwaggerOperation("Fazer upload de arquivo")]
    [ProducesResponseType(typeof(Response<UploadFileCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [DisableRequestSizeLimit] // Remove limite de tamanho para este endpoint (suporta arquivos grandes)
    public async Task<IActionResult> UploadFileAsync(IFormFile file)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        if (file == null || file.Length == 0)
        {
            return CreateResponse(BadRequest());
        }

        var command = new UploadFileCommand
        {
            File = file,
            UserData = userData
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<UploadFileCommandResult>(response)));
    }

    [HttpGet("upload/{stageId}/{fileName}")]
    [AllowAnonymous]
    [SwaggerOperation("Buscar arquivo físico pelo stageId e nome (UUID + extensão) - Público, sem autenticação")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileAsync(string stageId, string fileName)
    {
        // Validar parâmetros
        if (string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        try
        {
            // Buscar configurações do S3
            var s3Config = await _filesStorageConfigService.GetConfigurationAsync();

            // Se S3 estiver habilitado, buscar do S3
            if (s3Config.Status && !string.IsNullOrWhiteSpace(s3Config.Bucket))
            {
                _logger.LogInformation("Buscando arquivo do S3. Bucket: {Bucket}, StageId: {StageId}, FileName: {FileName}", 
                    s3Config.Bucket, stageId, fileName);
                
                return await GetFileFromS3Async(s3Config, stageId, fileName);
            }
            else
            {
                // Buscar do local (comportamento atual)
                _logger.LogInformation("Buscando arquivo local. StageId: {StageId}, FileName: {FileName}", stageId, fileName);
                
                return GetFileFromLocal(stageId, fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo. StageId: {StageId}, FileName: {FileName}", stageId, fileName);
            return NotFound();
        }
    }

    private IActionResult GetFileFromLocal(string stageId, string fileName)
    {
        // Construir caminho direto: uploads/stageId/fileName
        var uploadsPath = _configuration["FileUpload:Path"] ?? "uploads";
        var basePath = Path.Combine(_environment.ContentRootPath, uploadsPath);
        var stagePath = Path.Combine(basePath, stageId);
        var filePath = Path.Combine(stagePath, fileName);

        // Verificar se o arquivo existe
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // Obter ContentType baseado na extensão
        var contentType = GetContentType(fileName);

        // Remover qualquer header Content-Disposition que possa forçar download
        Response.Headers.Remove("Content-Disposition");

        // Retornar arquivo usando File com FileStream para garantir controle total
        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType);
    }

    private async Task<IActionResult> GetFileFromS3Async(
        FilesStorageConfiguration s3Config, 
        string stageId, 
        string fileName)
    {
        try
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

            // Buscar arquivo do S3
            var getRequest = new GetObjectRequest
            {
                BucketName = s3Config.Bucket,
                Key = s3Key
            };

            var getResponse = await s3Client.GetObjectAsync(getRequest);

            // Obter ContentType do S3 ou da extensão
            var contentType = !string.IsNullOrWhiteSpace(getResponse.Headers.ContentType) 
                ? getResponse.Headers.ContentType 
                : GetContentType(fileName);

            // Remover qualquer header Content-Disposition que possa forçar download
            Response.Headers.Remove("Content-Disposition");

            // Retornar arquivo do stream do S3
            return File(getResponse.ResponseStream, contentType);
        }
        catch (Amazon.S3.AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "Erro ao buscar arquivo do S3. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}", 
                s3Ex.StatusCode, s3Ex.ErrorCode);
            
            if (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            
            throw;
        }
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

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Buscar dados (metadata) do arquivo no MongoDB por id")]
    [ProducesResponseType(typeof(Response<GetMediaByIdCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileDataByIdAsync(string id)
    {
        // JWT obrigatório (controller está com [Authorize])
        var userData = _jwtUserService.GetUserData();
        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return CreateResponse(BadRequest());
        }

        var command = new GetMediaByIdCommand
        {
            Id = id
        };

        var response = await Bus.Send(command);
        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        return CreateResponse(Ok(new Response<GetMediaByIdCommandResult>(response)));
    }

    [HttpPut("{id}")]
    [SwaggerOperation("Atualizar informações do arquivo no MongoDB (tags, etc)")]
    [ProducesResponseType(typeof(Response<UpdateMediaCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMediaAsync(string id, [FromBody] UpdateMediaCommand command)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        // Validar id do MongoDB
        if (string.IsNullOrWhiteSpace(id))
        {
            return CreateResponse(BadRequest());
        }

        // Garantir que o ID da URL seja usado
        command.Id = id;
        command.UserData = userData;

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        return CreateResponse(Ok(new Response<UpdateMediaCommandResult>(response)));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation("Deletar arquivo físico e registro no MongoDB")]
    [ProducesResponseType(typeof(Response<DeleteFileCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFileAsync(string id)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        // Validar id do MongoDB
        if (string.IsNullOrWhiteSpace(id))
        {
            return CreateResponse(BadRequest());
        }

        var command = new DeleteFileCommand
        {
            Id = id,
            UserData = userData
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        return CreateResponse(Ok(new Response<DeleteFileCommandResult>(response)));
    }

    [HttpGet]
    [SwaggerOperation("Listar arquivos (media) paginados do stage atual do usuário com filtros e ordenação")]
    [ProducesResponseType(typeof(Response<GetMediaCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMediaAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] List<string>? tags = null,
        [FromQuery] string? contentType = null,
        [FromQuery] string? extension = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc")
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        // Validar parâmetros de paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limitar máximo

        // Validar sortBy
        var validSortBy = new[] { "createdAt", "name", "fileSize" };
        if (!validSortBy.Contains(sortBy?.ToLower()))
        {
            sortBy = "createdAt";
        }

        // Validar sortDirection
        var validSortDirection = new[] { "asc", "desc" };
        if (!validSortDirection.Contains(sortDirection?.ToLower()))
        {
            sortDirection = "desc";
        }

        var command = new GetMediaCommand
        {
            Page = page,
            PageSize = pageSize,
            Tags = tags,
            ContentType = contentType,
            Extension = extension,
            StageId = userData.StageId, // Filtrar apenas arquivos do stage do usuário
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var response = await Bus.Send(command);

        // Se response for null, retornar resultado vazio
        if (response == null)
        {
            response = new GetMediaCommandResult
            {
                Data = new List<MediaDto>(),
                Total = 0,
                Page = page,
                PageSize = pageSize,
                TotalPages = 0
            };
        }

        return CreateResponse(Ok(new Response<GetMediaCommandResult>(response)));
    }
}
