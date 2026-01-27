using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Application.Queries;
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

    [HttpPost("{id}/new")]
    [SwaggerOperation("Criar novo item em uma collection")]
    [ProducesResponseType(typeof(Response<CreateCollectionItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCollectionItemAsync(string id, [FromBody] CreateCollectionItemCommand command)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        // Preencher CollectionId a partir da URL antes de validar
        if (command == null)
        {
            command = new CreateCollectionItemCommand();
        }
        command.CollectionId = id;
        command.UserData = userData;

        // Validar se o CollectionId foi preenchido
        if (string.IsNullOrWhiteSpace(command.CollectionId))
        {
            return CreateResponse(BadRequest());
        }

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Created(string.Empty, new Response<CreateCollectionItemResponse>(response)));
    }

    [HttpGet("{id}/{itemId}")]
    [SwaggerOperation("Obter um item específico de uma collection")]
    [ProducesResponseType(typeof(Response<GetCollectionItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollectionItemAsync(string id, string itemId)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        var command = new GetCollectionItemCommand
        {
            CollectionId = id,
            ItemId = itemId
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(NotFound());
        }

        return CreateResponse(Ok(new Response<GetCollectionItemResponse>(response)));
    }

    [HttpGet("{id}/{itemId}/change")]
    [SwaggerOperation("Obter histórico de alterações de um item de collection")]
    [ProducesResponseType(typeof(Response<IEnumerable<GetCollectionItemChangesQueryResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCollectionItemChangesAsync(string id, string itemId)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(itemId))
        {
            return CreateResponse(BadRequest());
        }

        var query = new GetCollectionItemChangesQuery
        {
            CollectionId = id,
            ItemId = itemId
        };

        var response = await Bus.Send(query);

        return CreateResponse(Ok(new Response<IEnumerable<GetCollectionItemChangesQueryResult>>(response)));
    }

    [HttpGet("{id}/change")]
    [SwaggerOperation("Obter histórico de alterações de todos os itens de uma collection (paginado)")]
    [ProducesResponseType(typeof(Response<GetCollectionChangesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCollectionChangesAsync(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return CreateResponse(BadRequest());
        }

        // Validar parâmetros de paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limitar máximo

        var query = new GetCollectionChangesQuery
        {
            CollectionId = id,
            Page = page,
            PageSize = pageSize
        };

        var response = await Bus.Send(query);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<GetCollectionChangesQueryResult>(response)));
    }

    [HttpPut("{id}/{itemId}")]
    [SwaggerOperation("Atualizar item de uma collection")]
    [ProducesResponseType(typeof(Response<UpdateCollectionItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCollectionItemAsync(string id, string itemId, [FromBody] UpdateCollectionItemCommand command)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        // Preencher CollectionId e ItemId a partir da URL antes de validar
        if (command == null)
        {
            command = new UpdateCollectionItemCommand();
        }
        command.CollectionId = id;
        command.ItemId = itemId;
        command.UserData = userData;

        // Validar se os IDs foram preenchidos
        if (string.IsNullOrWhiteSpace(command.CollectionId) || string.IsNullOrWhiteSpace(command.ItemId))
        {
            return CreateResponse(BadRequest());
        }

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<UpdateCollectionItemResponse>(response)));
    }

    [HttpDelete("{id}/{itemId}")]
    [SwaggerOperation("Deletar item de uma collection")]
    [ProducesResponseType(typeof(Response<DeleteCollectionItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCollectionItemAsync(string id, string itemId)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        var command = new DeleteCollectionItemCommand
        {
            CollectionId = id,
            ItemId = itemId,
            UserData = userData
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<DeleteCollectionItemResponse>(response)));
    }

    [HttpGet("{id}/items")]
    [SwaggerOperation("Listar todos os itens de uma collection")]
    [ProducesResponseType(typeof(Response<GetCollectionItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollectionItemsAsync(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

        var command = new GetCollectionItemsCommand
        {
            CollectionId = id,
            Page = page,
            PageSize = pageSize
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        // Se não houver dados, retornar resultado vazio (mas manter o CollectionName que já vem do handler)
        if (response.Items == null || response.Items.Count == 0)
        {
            response.Items = new List<Dictionary<string, object>>();
        }

        return CreateResponse(Ok(new Response<GetCollectionItemsResponse>(response)));
    }

    [HttpPut("{id}")]
    [SwaggerOperation("Atualizar uma collection")]
    [ProducesResponseType(typeof(Response<UpdateCollectionCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCollectionAsync(string id, [FromBody] UpdateCollectionCommand command)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        // Preencher CollectionId e UserData a partir da URL
        if (command == null)
        {
            command = new UpdateCollectionCommand();
        }

        command.CollectionId = id;
        command.UserData = userData;

        // Validar se o CollectionId foi preenchido
        if (string.IsNullOrWhiteSpace(command.CollectionId))
        {
            return CreateResponse(BadRequest());
        }

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<UpdateCollectionCommandResult>(response)));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation("Deletar uma collection")]
    [ProducesResponseType(typeof(Response<DeleteCollectionCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCollectionAsync(string id)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid() || string.IsNullOrWhiteSpace(userData.StageId))
        {
            return CreateResponse(Unauthorized());
        }

        var command = new DeleteCollectionCommand
        {
            CollectionId = id
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<DeleteCollectionCommandResult>(response)));
    }
}
