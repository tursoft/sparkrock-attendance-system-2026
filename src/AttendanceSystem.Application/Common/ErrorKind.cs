namespace AttendanceSystem.Application.Common;

/// <summary>The category of an expected failure. The API maps each kind to one HTTP status code.</summary>
public enum ErrorKind
{
    NotFound,
    Validation,
    Conflict,
}
