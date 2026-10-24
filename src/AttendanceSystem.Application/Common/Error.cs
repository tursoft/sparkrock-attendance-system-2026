using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace AttendanceSystem.Application.Common;

/// <summary>An expected failure of a use case: not found, invalid input or a conflicting concurrent change.</summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "C#-only solution; Error is the conventional name next to Result.")]
public sealed class Error
{
    public const string ValidationCode = "validation";
    private const string ValidationMessage = "One or more validation errors occurred.";

    private Error(string code, string message, ErrorKind kind, IReadOnlyDictionary<string, string[]> validationErrors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
        Kind = kind;
        ValidationErrors = validationErrors;
    }

    /// <summary>A stable, machine-readable identifier such as <c>school.not_found</c>.</summary>
    public string Code { get; }

    /// <summary>A safe, human-readable explanation that may be returned to the client.</summary>
    public string Message { get; }

    public ErrorKind Kind { get; }

    /// <summary>Messages per request field (for example <c>entries[2].code</c>); empty unless <see cref="Kind"/> is <see cref="ErrorKind.Validation"/>.</summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorKind.NotFound, ReadOnlyDictionary<string, string[]>.Empty);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorKind.Conflict, ReadOnlyDictionary<string, string[]>.Empty);

    public static Error Validation(string field, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return Validation(new Dictionary<string, string[]> { [field] = [message] });
    }

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("A validation error needs at least one field.", nameof(errors));
        }

        var copy = errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        return new(ValidationCode, ValidationMessage, ErrorKind.Validation, copy.AsReadOnly());
    }
}
