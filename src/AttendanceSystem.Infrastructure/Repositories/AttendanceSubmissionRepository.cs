using AttendanceSystem.Domain.Attendance;
using AttendanceSystem.Infrastructure.Mapping;
using AttendanceSystem.Infrastructure.Persistence;

namespace AttendanceSystem.Infrastructure.Repositories;

internal sealed class AttendanceSubmissionRepository(AttendanceDbContext db) : IAttendanceSubmissionRepository
{
    public void Add(AttendanceSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);
        db.AttendanceSubmissionLogs.Add(submission.ToEntity());
    }
}
