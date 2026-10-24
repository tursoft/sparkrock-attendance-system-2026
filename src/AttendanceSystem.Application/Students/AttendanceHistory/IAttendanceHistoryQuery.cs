using AttendanceSystem.Application.Common;
using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Application.Students.AttendanceHistory;

public interface IAttendanceHistoryQuery
{
    /// <summary>
    /// One page of the student's records, newest first, at every school, keeping each record that falls
    /// within its own school's range for the year (#44).
    /// </summary>
    Task<PagedResult<AttendanceHistoryItemDto>> GetPageAsync(
        int studentId, SchoolYear schoolYear, PageRequest page, CancellationToken cancellationToken);
}
