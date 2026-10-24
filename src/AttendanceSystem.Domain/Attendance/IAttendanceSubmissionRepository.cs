namespace AttendanceSystem.Domain.Attendance;

public interface IAttendanceSubmissionRepository
{
    void Add(AttendanceSubmission submission);
}
