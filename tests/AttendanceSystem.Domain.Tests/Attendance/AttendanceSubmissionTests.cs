using AttendanceSystem.Domain.Attendance;

namespace AttendanceSystem.Domain.Tests.Attendance;

public class AttendanceSubmissionTests
{
    private static readonly DateOnly AttendDate = new(2025, 10, 6);
    private static readonly DateTime SubmittedAt = new(2025, 10, 6, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Keeps_its_details()
    {
        var submission = new AttendanceSubmission(3, AttendDate, 25, SubmittedAt, "attendance-api");

        Assert.Equal(3, submission.SchoolId);
        Assert.Equal(AttendDate, submission.AttendDate);
        Assert.Equal(25, submission.RecordCount);
        Assert.Equal(SubmittedAt, submission.SubmittedAt);
        Assert.Equal("attendance-api", submission.SubmittedBy);
    }

    [Fact]
    public void Accepts_a_submission_with_no_records()
    {
        Assert.Equal(0, new AttendanceSubmission(3, AttendDate, 0, SubmittedAt, "attendance-api").RecordCount);
    }

    [Fact]
    public void Rejects_a_negative_record_count()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AttendanceSubmission(3, AttendDate, -1, SubmittedAt, "attendance-api"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Requires_the_submitting_user(string? user)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AttendanceSubmission(3, AttendDate, 1, SubmittedAt, user!));
    }
}
