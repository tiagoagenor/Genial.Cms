using System;
using Genial.Cms.Factories;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Filters;

public class GlobalExceptionFilterAttribute : Attribute, IExceptionFilter
{
    private readonly ILogger _logger;

    public GlobalExceptionFilterAttribute(
        ILogger logger
    )
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var eventId = new EventId(188, "GlobalException");

        _logger.LogError(eventId, context.Exception, context.Exception.Message);

        var problemDetails = CustomProblemDetailsFactory.CreateProblemDetailsFromContext(
            context.HttpContext,
            "An internal error occurred while processing your request",
            "188"
        );

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
