using AttendanceSystem.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Api.ErrorHandling;

/// <summary>Turns every exception that escapes an endpoint into ProblemDetails without leaking internals.</summary>
internal sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badField && BadRequestErrors.FieldErrors(badField) is { } fieldErrors)
        {
            LogRequestRejected(logger, StatusCodes.Status400BadRequest, exception.Message);
            await ValidationProblems.Create(fieldErrors).ExecuteAsync(httpContext);
            return true;
        }

        var problem = exception switch
        {
            DomainException => ClientProblem(StatusCodes.Status400BadRequest, exception.Message),
            BadHttpRequestException badRequest => ClientProblem(badRequest.StatusCode, "The request could not be read. Check the route values, query string and body."),
            _ => UnexpectedProblem(httpContext, exception),
        };

        // Writes problem+json even when the client's Accept header rules out the registered writers.
        await TypedResults.Problem(problem).ExecuteAsync(httpContext);
        return true;

        ProblemDetails ClientProblem(int statusCode, string detail)
        {
            LogRequestRejected(logger, statusCode, exception.Message);
            return new ProblemDetails { Status = statusCode, Detail = detail };
        }
    }

    private ProblemDetails UnexpectedProblem(HttpContext httpContext, Exception exception)
    {
        LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path.Value);
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "The server could not complete the request. Quote the traceId when reporting this problem.",
        };
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request rejected with {StatusCode}: {Reason}")]
    private static partial void LogRequestRejected(ILogger logger, int statusCode, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception while processing {RequestMethod} {RequestPath}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string requestMethod, string? requestPath);
}
