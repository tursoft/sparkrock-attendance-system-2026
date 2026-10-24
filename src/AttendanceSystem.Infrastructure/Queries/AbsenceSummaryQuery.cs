using AttendanceSystem.Application.Students.AbsenceSummary;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Queries;

public sealed class AbsenceSummaryQuery(AttendanceDbContext db) : IAbsenceSummaryQuery
{
    public async Task<int> GetTotalAbsencesAsync(
        int studentId, int schoolId, SchoolYear schoolYear, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);

        var label = schoolYear.ToString();
        return await db.StudentAttendanceSummaries
            .AsNoTracking()
            .Where(summary => summary.StudentId == studentId && summary.SchoolId == schoolId && summary.SchoolYear == label)
            .Select(summary => (int?)summary.TotalAbsences)
            .SingleOrDefaultAsync(cancellationToken) ?? 0;
    }
}
