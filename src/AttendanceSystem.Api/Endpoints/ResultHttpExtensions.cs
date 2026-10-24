using System.Diagnostics;
using AttendanceSystem.Application.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AttendanceSystem.Api.Endpoints;

/// <summary>The one mapping from application results to HTTP: NotFound → 404, Validation → 400, Conflict → 409.</summary>
public static class ResultHttpExtensions
{
    public static Results<TSuccess, ValidationProblem, ProblemHttpResult> ToHttpResult<TValue, TSuccess>(
        this Result<TValue> result, Func<TValue, TSuccess> onSuccess)
        where TSuccess : IResult
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : ToProblem<TSuccess>(result.Error);
    }

    public static Results<TSuccess, ValidationProblem, ProblemHttpResult> ToHttpResult<TSuccess>(
        this Result result, Func<TSuccess> onSuccess)
        where TSuccess : IResult
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess() : ToProblem<TSuccess>(result.Error);
    }

    private static Results<TSuccess, ValidationProblem, ProblemHttpResult> ToProblem<TSuccess>(Error error)
        where TSuccess : IResult
    {
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        return error.Kind switch
        {
            ErrorKind.Validation => TypedResults.ValidationProblem(error.ValidationErrors.ToDictionary(), extensions: extensions),
            ErrorKind.NotFound => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound, extensions: extensions),
            ErrorKind.Conflict => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status409Conflict, extensions: extensions),
            _ => throw new UnreachableException($"Unmapped error kind {error.Kind}."),
        };
    }
}
