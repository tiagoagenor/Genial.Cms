using Genial.Cms.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Genial.Cms.Factories;

public static class CustomProblemDetailsFactory
{
    private const string ErrorTitle = "It was not possible to process your request";
    private const string ClientErrorDetail = "Please refer to the errors property for additional details.";
    private const string ServerErrorDetail = "There's an problem in the server, please try again later";

    public static ProblemDetails CreateProblemDetailsFromContext(HttpContext httpContext, ExceptionNotificationHandler exceptionHandler)
    {
        var isClientError = exceptionHandler.GetExceptionType() is ExceptionType.Client;

        return new ProblemDetails
        {
            Title = ErrorTitle,
            Detail = isClientError ? ClientErrorDetail : ServerErrorDetail,
            Status = isClientError ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path.ToString(),
            Extensions =
            {
                { "success", false },
                { "traceId", httpContext.TraceIdentifier },
                { "errors", exceptionHandler.GetNotifications() }
            }
        };
    }

    public static ProblemDetails CreateProblemDetailsFromContext(HttpContext httpContext, string detail, string code)
    {
        return new ProblemDetails
        {
            Title = ErrorTitle,
            Detail = detail,
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path.ToString(),
            Extensions =
            {
                { "success", false },
                { "traceId", httpContext.TraceIdentifier },
                { "code", code }
            }
        };
    }
}
