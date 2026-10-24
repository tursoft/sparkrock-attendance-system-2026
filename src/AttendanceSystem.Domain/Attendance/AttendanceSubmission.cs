namespace AttendanceSystem.Domain.Attendance;

/// <summary>The audit entry logged for every daily attendance save, even one with no records.</summary>
public sealed class AttendanceSubmission
{
    public AttendanceSubmission(int schoolId, DateOnly attendDate, int recordCount, DateTime submittedAt, string submittedBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(recordCount);
        ArgumentException.ThrowIfNullOrWhiteSpace(submittedBy);

        SchoolId = schoolId;
        AttendDate = attendDate;
        RecordCount = recordCount;
        SubmittedAt = submittedAt;
        SubmittedBy = submittedBy;
    }

    public int SchoolId { get; }

    public DateOnly AttendDate { get; }

    public int RecordCount { get; }

    public DateTime SubmittedAt { get; }

    public string SubmittedBy { get; }
}
