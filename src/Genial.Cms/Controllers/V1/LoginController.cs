using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Login;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
public class LoginController : BaseController
{
    public LoginController(INotificationHandler<ExceptionNotification> notifications, IMediator bus)
        : base(notifications, bus)
    {
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
}

