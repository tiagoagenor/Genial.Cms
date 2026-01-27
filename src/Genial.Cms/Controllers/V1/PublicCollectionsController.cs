using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/api/collection")]
public class PublicCollectionsController : BaseController
{
    public PublicCollectionsController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus)
        : base(notifications, bus)
    {
    }

    [HttpGet("{stage}/{slug}")]
    [SwaggerOperation("Obter itens de uma collection por stage e slug (público, sem autenticação)")]
    [ProducesResponseType(typeof(Response<GetCollectionItemsBySlugResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollectionItemsBySlugAsync(string stage, string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Validar parâmetros de paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limitar máximo

        var command = new GetCollectionItemsBySlugCommand
        {
            StageKey = stage,
            Slug = slug,
            Page = page,
            PageSize = pageSize
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        // Se não houver dados, retornar resultado vazio
        if (response.Items == null || response.Items.Count == 0)
        {
            response.Items = new List<Dictionary<string, object>>();
        }

        return CreateResponse(Ok(new Response<GetCollectionItemsBySlugResponse>(response)));
    }
}
