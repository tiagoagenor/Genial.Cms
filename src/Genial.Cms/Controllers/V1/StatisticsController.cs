using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
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
[Route("v{version:apiVersion}/statistics")]
[Authorize]
public class StatisticsController : BaseController
{
    public StatisticsController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus)
        : base(notifications, bus)
    {
    }

    [HttpGet("by-stage")]
    [SwaggerOperation("Obter estatísticas de dados por stage")]
    [ProducesResponseType(typeof(Response<IEnumerable<GetStatisticsByStageQueryResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatisticsByStageAsync()
    {
        var response = await Bus.Send(new GetStatisticsByStageQuery());
        return CreateResponse(Ok(new Response<IEnumerable<GetStatisticsByStageQueryResult>>(response)));
    }
}
