namespace AttendanceSystem.Domain.Attendance;

public sealed class AttendanceCode
{
    public AttendanceCode(string value, string description, bool isAbsent, bool isExcused, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(description);

        Value = value;
        Description = description;
        IsAbsent = isAbsent;
        IsExcused = isExcused;
        IsActive = isActive;
    }

    public string Value { get; }

    public string Description { get; }

    public bool IsAbsent { get; }

    public bool IsExcused { get; }

    public bool IsActive { get; }
}
