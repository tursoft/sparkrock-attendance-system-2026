using AttendanceSystem.Application.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AttendanceSystem.Api.ErrorHandling;

/// <summary>The one shape of every 400: field errors, <c>code</c> "validation" and, via IProblemDetailsService, a <c>traceId</c>.</summary>
public static class ValidationProblems
{
    // TypedResults.Problem goes through IProblemDetailsService (adds traceId); TypedResults.ValidationProblem doesn't in .NET 8.
    public static ProblemHttpResult Create(IDictionary<string, string[]> errors) =>
        TypedResults.Problem(new HttpValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Extensions = { ["code"] = Error.ValidationCode },
        });
}
