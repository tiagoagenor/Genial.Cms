using System.Collections.Generic;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Stage;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.Aggregates;
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
public class StagesController : BaseController
{
    private readonly IJwtUserService _jwtUserService;

    public StagesController(
        INotificationHandler<ExceptionNotification> notifications,
        IMediator bus,
        IJwtUserService jwtUserService)
        : base(notifications, bus)
    {
        _jwtUserService = jwtUserService;
    }

    [HttpGet]
    [SwaggerOperation("Obter todos os stages")]
    [ProducesResponseType(typeof(Response<IEnumerable<Stage>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStagesAsync()
    {
        var response = await Bus.Send(new GetStagesQuery());
        return CreateResponse(Ok(new Response<IEnumerable<Stage>>(response)));
    }

    [HttpPost("change")]
    [SwaggerOperation("Trocar de stage")]
    [ProducesResponseType(typeof(Response<ChangeStageCommandResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeStageAsync([FromBody] ChangeStageRequest request)
    {
        // Obter informações do usuário do JWT atual
        var userData = _jwtUserService.GetUserData();

        if (!userData.IsValid())
        {
            return CreateResponse(Unauthorized());
        }

        var command = new ChangeStageCommand
        {
            StageId = request.StageId,
            UserData = userData
        };

        var response = await Bus.Send(command);

        if (response == null)
        {
            return CreateResponse(BadRequest());
        }

        return CreateResponse(Ok(new Response<ChangeStageCommandResult>(response)));
    }
}

public class ChangeStageRequest
{
    public string StageId { get; set; }
}

