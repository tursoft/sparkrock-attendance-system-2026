using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Api.ErrorHandling;

/// <summary>Applied to every ProblemDetails the API writes, whatever produced it.</summary>
internal static class ProblemDetailsCustomization
{
    public static void Apply(ProblemDetailsContext context)
    {
        context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

        if (context.ProblemDetails is HttpValidationProblemDetails validation)
        {
            var namingPolicy = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions.PropertyNamingPolicy;
            NameErrorsLikeJson(validation, namingPolicy);
        }
    }

    // Built-in DTO validation keys errors by CLR member path (Entries[1].StudentId); clients send and
    // receive JSON names (entries[1].studentId), which is also how application errors are keyed.
    private static void NameErrorsLikeJson(HttpValidationProblemDetails validation, JsonNamingPolicy? namingPolicy)
    {
        if (namingPolicy is null)
        {
            return;
        }

        validation.Errors = validation.Errors
            .GroupBy(error => ToJsonPath(error.Key, namingPolicy), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.SelectMany(error => error.Value).ToArray(), StringComparer.Ordinal);
    }

    private static string ToJsonPath(string memberPath, JsonNamingPolicy namingPolicy) =>
        string.Join('.', memberPath.Split('.').Select(namingPolicy.ConvertName));
}
