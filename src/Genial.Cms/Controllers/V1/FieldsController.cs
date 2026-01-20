using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Field;
using Genial.Cms.Domain.Aggregates;
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
[Authorize]
public class FieldsController : BaseController
{
    public FieldsController(INotificationHandler<ExceptionNotification> notifications, IMediator bus)
        : base(notifications, bus)
    {
    }

    [HttpPost]
    [SwaggerOperation("Criar novo field")]
    [ProducesResponseType(typeof(Response<CreateFieldCommandResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFieldAsync([FromBody] CreateFieldCommand command)
    {
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Created(string.Empty, new Response<CreateFieldCommandResult>(response)));
    }
}

