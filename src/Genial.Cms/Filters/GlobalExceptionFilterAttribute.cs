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
    private readonly ILogger<GlobalExceptionFilterAttribute> _logger;

    public GlobalExceptionFilterAttribute(
        ILogger<GlobalExceptionFilterAttribute> logger
    )
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var eventId = new EventId(188, "GlobalException");
        var exception = context.Exception;

        // Log completo da exceção com stack trace
        _logger.LogError(
            eventId,
            exception,
            "Exception occurred: {Message}\nStack Trace: {StackTrace}",
            exception.Message,
            exception.StackTrace
        );

        // Log adicional para exceções internas
        if (exception.InnerException != null)
        {
            _logger.LogError(
                "Inner Exception: {InnerMessage}\nInner Stack Trace: {InnerStackTrace}",
                exception.InnerException.Message,
                exception.InnerException.StackTrace
            );
        }

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
