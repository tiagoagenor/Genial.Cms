using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Dtos;
using Genial.Cms.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
[Authorize]
public class CollectionsController : BaseController
{
    private readonly IJwtUserService _jwtUserService;

    public CollectionsController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus,
        IJwtUserService jwtUserService)
        : base(notifications, bus)
    {
        _jwtUserService = jwtUserService;
    }

    [HttpPost]
    [SwaggerOperation("Criar nova collection")]
    [ProducesResponseType(typeof(Response<CreateCollectionCommandResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCollectionAsync([FromBody] CreateCollectionCommand command)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        // Adicionar dados do usuário ao comando
        command.UserData = userData;

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Created(string.Empty, new Response<CreateCollectionCommandResult>(response)));
    }

    [HttpGet]
    [SwaggerOperation("Listar todas as collections do stage atual")]
    [ProducesResponseType(typeof(Response<GetCollectionsCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCollectionsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        // Validar parâmetros de paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limitar máximo

        var command = new GetCollectionsCommand
        {
            StageId = userData.StageId,
            Page = page,
            PageSize = pageSize
        };

        var response = await Bus.Send(command);

        // Se response for null, retornar resultado vazio
        if (response == null)
        {
            response = new GetCollectionsCommandResult
            {
                Data = new List<CollectionNameDto>(),
                Total = 0,
                Page = page,
                PageSize = pageSize,
                TotalPages = 0
            };
        }

        return CreateResponse(Ok(new Response<GetCollectionsCommandResult>(response)));
    }

    [HttpGet("{id}/fields")]
    [SwaggerOperation("Listar todos os fields de uma collection")]
    [ProducesResponseType(typeof(Response<GetCollectionFieldsCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollectionFieldsAsync(string id)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        var command = new GetCollectionFieldsCommand
        {
            CollectionId = id
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        return CreateResponse(Ok(new Response<GetCollectionFieldsCommandResult>(response)));
    }
}
