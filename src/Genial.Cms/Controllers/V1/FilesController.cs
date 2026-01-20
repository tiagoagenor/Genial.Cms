using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.TypeFile;
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
[Route("v{version:apiVersion}/files")]
[Authorize]
public class FilesController : BaseController
{
    public FilesController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus)
        : base(notifications, bus)
    {
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
}
