using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Configuration;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[Authorize]
public class ConfigurationController : BaseController
{
    private readonly IFilesStorageConfigurationService _filesStorageConfigurationService;
    private readonly IRedisConfigurationService _redisConfigurationService;

    public ConfigurationController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus,
        IFilesStorageConfigurationService filesStorageConfigurationService,
        IRedisConfigurationService redisConfigurationService)
        : base(notifications, bus)
    {
        _filesStorageConfigurationService = filesStorageConfigurationService;
        _redisConfigurationService = redisConfigurationService;
    }

    [HttpGet("FilesStorage")]
    [SwaggerOperation("Obter configuração de armazenamento de arquivos (S3)")]
    [ProducesResponseType(typeof(Response<GetFilesStorageConfigurationCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFilesStorageConfigurationAsync()
    {
        var response = await Bus.Send(new GetFilesStorageConfigurationCommand());

        // Sempre retorna sucesso, mesmo que não exista no banco (retorna valores padrão)
        return CreateResponse(Ok(new Response<GetFilesStorageConfigurationCommandResult>(response)));
    }

    [HttpPost("FilesStorage")]
    [SwaggerOperation("Salvar/Atualizar configuração de armazenamento de arquivos (S3)")]
    [ProducesResponseType(typeof(Response<SaveFilesStorageConfigurationCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveFilesStorageConfigurationAsync([FromBody] SaveFilesStorageConfigurationCommand command)
    {
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<SaveFilesStorageConfigurationCommandResult>(response)));
    }

    [HttpGet("FilesStorage/test")]
    [SwaggerOperation("Testar conexão com S3")]
    [ProducesResponseType(typeof(Response<TestFilesStorageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TestFilesStorageAsync()
    {
        var isConnected = await _filesStorageConfigurationService.TestConnectionAsync();
        
        return CreateResponse(Ok(new Response<TestFilesStorageResponse>(new TestFilesStorageResponse
        {
            Success = isConnected
        })));
    }

    [HttpGet("Redis")]
    [SwaggerOperation("Obter configuração do Redis")]
    [ProducesResponseType(typeof(Response<GetRedisConfigurationCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRedisConfigurationAsync()
    {
        var response = await Bus.Send(new GetRedisConfigurationCommand());

        // Sempre retorna sucesso, mesmo que não exista no banco (retorna valores padrão)
        return CreateResponse(Ok(new Response<GetRedisConfigurationCommandResult>(response)));
    }

    [HttpPost("Redis")]
    [SwaggerOperation("Salvar/Atualizar configuração do Redis")]
    [ProducesResponseType(typeof(Response<SaveRedisConfigurationCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveRedisConfigurationAsync([FromBody] SaveRedisConfigurationCommand command)
    {
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<SaveRedisConfigurationCommandResult>(response)));
    }

    [HttpGet("Redis/test")]
    [SwaggerOperation("Testar conexão com Redis")]
    [ProducesResponseType(typeof(Response<TestRedisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TestRedisAsync()
    {
        var isConnected = await _redisConfigurationService.TestConnectionAsync();
        
        return CreateResponse(Ok(new Response<TestRedisResponse>(new TestRedisResponse
        {
            Success = isConnected
        })));
    }
}

public class TestFilesStorageResponse
{
    public bool Success { get; set; }
}

public class TestRedisResponse
{
    public bool Success { get; set; }
}
