using System.Text.Json;
using System.Text.RegularExpressions;

namespace AttendanceSystem.Api.ErrorHandling;

/// <summary>Names the request field behind a binding failure, so the client learns which value it must fix.</summary>
internal static partial class BadRequestErrors
{
    private const string JsonPathRoot = "$.";

    /// <summary>Field errors keyed like validation errors (<c>entries[0].studentId</c>, <c>schoolId</c>), or null when no field can be named.</summary>
    public static Dictionary<string, string[]>? FieldErrors(BadHttpRequestException exception)
    {
        if (exception.InnerException is JsonException { Path: { } path } && path.StartsWith(JsonPathRoot, StringComparison.Ordinal))
        {
            return Error(path[JsonPathRoot.Length..], "The value is not valid JSON of the expected type.");
        }

        // The framework's route and query binding message: Failed to bind parameter "{type} {name}" from "{value}".
        var parameter = BindParameterFailure().Match(exception.Message);
        return parameter.Success ? Error(parameter.Groups["name"].Value, "The value is not valid for this parameter.") : null;
    }

    private static Dictionary<string, string[]> Error(string field, string message) => new() { [field] = [message] };

    [GeneratedRegex("""^Failed to bind parameter "[^"]* (?<name>\w+)" from """)]
    private static partial Regex BindParameterFailure();
}
