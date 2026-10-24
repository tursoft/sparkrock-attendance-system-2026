namespace AttendanceSystem.Domain.Students;

public sealed class Student(int id, int schoolId, bool isActive)
{
    public int Id { get; } = id;

    /// <summary>The school the student currently attends.</summary>
    public int SchoolId { get; } = schoolId;

    public bool IsActive { get; } = isActive;

    /// <summary>Only active students of the school can have attendance saved there.</summary>
    public bool IsEnrolledAt(int schoolId) => IsActive && SchoolId == schoolId;
}
