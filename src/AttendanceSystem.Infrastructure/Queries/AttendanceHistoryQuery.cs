using AttendanceSystem.Application.Common;
using AttendanceSystem.Application.Students.AttendanceHistory;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Queries;

public sealed class AttendanceHistoryQuery(AttendanceDbContext db) : IAttendanceHistoryQuery
{
    public Task<PagedResult<AttendanceHistoryItemDto>> GetPageAsync(
        int studentId, SchoolYear schoolYear, PageRequest page, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);
        ArgumentNullException.ThrowIfNull(page);

        var label = schoolYear.ToString();

        // Each school's range for the year: its earliest term start to its latest term end (#30).
        var yearRanges = db.SchoolTerms
            .Where(term => term.SchoolYear == label)
            .GroupBy(term => term.SchoolId)
            .Select(year => new
            {
                SchoolId = year.Key,
                StartDate = year.Min(term => term.StartDate),
                EndDate = year.Max(term => term.EndDate),
            });

        var records =
            from record in db.StudentAttendances.AsNoTracking()
            join year in yearRanges on record.SchoolId equals year.SchoolId
            where record.StudentId == studentId
                && record.AttendDate >= year.StartDate
                && record.AttendDate <= year.EndDate
            orderby record.AttendDate descending
            select record;

        return records.ProjectToHistoryItem().ToPagedResultAsync(page, cancellationToken);
    }
}
