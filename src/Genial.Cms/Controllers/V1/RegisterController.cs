using System.Threading.Tasks;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
public class RegisterController : BaseController
{
    public RegisterController(INotificationHandler<ExceptionNotification> notifications, IMediator bus) 
        : base(notifications, bus)
    {
    }

    [HttpPost]
    [SwaggerOperation("Registrar novo usuário")]
    [ProducesResponseType(typeof(Response<RegisterUserCommandResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserCommand command)
    {
        var response = await Bus.Send(command);
        
        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Created(string.Empty, new Response<RegisterUserCommandResult>(response)));
    }
}

