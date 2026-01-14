using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Factories;
using Genial.Cms.Filters;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Genial.Cms.Controllers;

[Route("[controller]/v{version:apiVersion}")]
[ServiceFilter(typeof(GlobalExceptionFilterAttribute))]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public abstract class BaseController : Controller
{
    protected readonly IMediator Bus;
    private readonly ExceptionNotificationHandler _exceptionNotificationHandler;

    protected BaseController(INotificationHandler<ExceptionNotification> notifications, IMediator bus)
    {
        Bus = bus;
        _exceptionNotificationHandler = (ExceptionNotificationHandler)notifications;
    }

    private ProblemDetails GetProblem()
    {
        if (!_exceptionNotificationHandler.HasNotifications()) return default;

        return CustomProblemDetailsFactory.CreateProblemDetailsFromContext(
            HttpContext,
            _exceptionNotificationHandler
        );
    }

    protected IActionResult CreateResponse(IActionResult action)
    {
        var problem = GetProblem();

        if (problem is not null)
        {
            return new ObjectResult(problem)
            {
                StatusCode = problem.Status
            };
        }

        return action;
    }
}
