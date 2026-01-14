using System.Collections.Generic;
using Genial.Cms.Domain.Exceptions;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
public class SampleController : BaseController
{
    public SampleController(INotificationHandler<ExceptionNotification> notifications, IMediator bus) : base(notifications, bus)
    {
    }

    [HttpGet]
    [SwaggerOperation("Sample endpoint")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync([FromQuery] bool forceClientException)
    {
        var response = await Bus.Send(new SampleQuery(forceClientException));
        return CreateResponse(Ok(new Response<IEnumerable<string>>(response)));
    }
}
