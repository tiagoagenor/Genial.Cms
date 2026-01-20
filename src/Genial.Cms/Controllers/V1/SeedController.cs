using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
public class SeedController : BaseController
{
    public SeedController(INotificationHandler<ExceptionNotification> notifications, IMediator bus)
        : base(notifications, bus)
    {
    }

    [HttpPost]
    [SwaggerOperation("Executar seed do banco de dados - Criar usuário base")]
    [ProducesResponseType(typeof(Response<SeedCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SeedAsync()
    {
        var response = await Bus.Send(new SeedCommand());

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<SeedCommandResult>(response)));
    }
}

