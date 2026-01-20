using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.User;
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
[Authorize]
public class UsersController : BaseController
{
    public UsersController(INotificationHandler<ExceptionNotification> notifications, IMediator bus)
        : base(notifications, bus)
    {
    }

    [HttpPost]
    [SwaggerOperation("Criar novo usuário")]
    [ProducesResponseType(typeof(Response<CreateUserCommandResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserCommand command)
    {
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Created(string.Empty, new Response<CreateUserCommandResult>(response)));
    }

    [HttpGet]
    [SwaggerOperation("Listar todos os usuários")]
    [ProducesResponseType(typeof(Response<IEnumerable<GetUsersQueryResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsersAsync()
    {
        var response = await Bus.Send(new GetUsersQuery());
        return CreateResponse(Ok(new Response<IEnumerable<GetUsersQueryResult>>(response)));
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Obter usuário por ID")]
    [ProducesResponseType(typeof(Response<GetUserCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserAsync(string id)
    {
        var response = await Bus.Send(new GetUserCommand { Id = id });

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<GetUserCommandResult>(response)));
    }

    [HttpPut("{id}")]
    [SwaggerOperation("Atualizar usuário")]
    [ProducesResponseType(typeof(Response<UpdateUserCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUserAsync(string id, [FromBody] UpdateUserCommand command)
    {
        command.Id = id;
        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<UpdateUserCommandResult>(response)));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation("Excluir usuário")]
    [ProducesResponseType(typeof(Response<DeleteUserCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUserAsync(string id)
    {
        var response = await Bus.Send(new DeleteUserCommand { Id = id });

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<DeleteUserCommandResult>(response)));
    }
}
