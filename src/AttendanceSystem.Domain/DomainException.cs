namespace AttendanceSystem.Domain;

/// <summary>A business invariant was violated. The API maps it to 400 ProblemDetails.</summary>
public sealed class DomainException(string message) : Exception(message);
