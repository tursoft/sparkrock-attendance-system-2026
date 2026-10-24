using System.Diagnostics;

namespace AttendanceSystem.Api.ErrorHandling;

/// <summary>Applied to every ProblemDetails the API writes, whatever produced it.</summary>
internal static class ProblemDetailsCustomization
{
    public static void Apply(ProblemDetailsContext context) =>
        context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
}
