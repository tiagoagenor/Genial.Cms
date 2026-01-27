using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Login;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
public class LoginController : BaseController
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus,
        IJwtTokenService jwtTokenService,
        ILogger<LoginController> logger)
        : base(notifications, bus)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerOperation("Realizar login do usuário")]
    [ProducesResponseType(typeof(Response<LoginCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginCommand command)
    {
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(Unauthorized());
        }

        return CreateResponse(Ok(new Response<LoginCommandResult>(response)));
    }

    [HttpPost("refresh")]
    [Produces("application/json")]
    [SwaggerOperation("Renovar token JWT a partir do header Authorization")]
    [ProducesResponseType(typeof(Response<RefreshTokenCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync()
    {
        // Extrair token do header Authorization
        var authHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            _logger.LogWarning("Token não fornecido no header Authorization");
            await Bus.Publish(new ExceptionNotification("008", "Token é obrigatório no header Authorization.", ExceptionType.Client, "Authorization"));
            return CreateResponse(Unauthorized());
        }

        // Remover "Bearer " do início se existir
        var token = authHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring(7)
            : authHeader;

        try
        {
            // Gerar novo token com nova data de expiração
            var newToken = _jwtTokenService.RefreshToken(token);

            _logger.LogInformation("Token renovado com sucesso");

            var result = new RefreshTokenCommandResult
            {
                Token = newToken
            };

            return CreateResponse(Ok(new Response<RefreshTokenCommandResult>(result)));
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token inválido fornecido para refresh");
            await Bus.Publish(new ExceptionNotification("009", "Token inválido ou malformado.", ExceptionType.Client, "Authorization"));
            return CreateResponse(Unauthorized());
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar refresh de token");
            await Bus.Publish(new ExceptionNotification("010", "Erro ao processar renovação de token.", ExceptionType.Server));
            return CreateResponse(Unauthorized());
        }
    }
}

