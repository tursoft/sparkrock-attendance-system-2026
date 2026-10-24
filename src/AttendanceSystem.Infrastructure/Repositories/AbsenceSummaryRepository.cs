using AttendanceSystem.Domain.AbsenceSummaries;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Infrastructure.Mapping;
using AttendanceSystem.Infrastructure.Persistence;
using AttendanceSystem.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories;

internal sealed class AbsenceSummaryRepository(AttendanceDbContext db) : IAbsenceSummaryRepository
{
    /// <remarks>Summaries are tracked, so <see cref="Update"/> writes back to the same row and its row version guards the race (#41).</remarks>
    public async Task<IReadOnlyList<AbsenceSummary>> GetManyAsync(
        int schoolId, SchoolYear schoolYear, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);
        var year = schoolYear.ToString();
        var rows = await db.StudentAttendanceSummaries
            .Where(s => s.SchoolId == schoolId && s.SchoolYear == year && studentIds.Contains(s.StudentId))
            .Select(s => new
            {
                Summary = s,
                OpenAlert = db.StudentAlerts
                    .Where(a => a.StudentId == s.StudentId
                        && a.SchoolId == s.SchoolId
                        && a.SchoolYear == s.SchoolYear
                        && a.AlertType == ChronicAbsenceAlert.TypeName
                        && a.ResolvedDate == null)
                    .OrderBy(a => a.AlertId)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
        return [.. rows.Select(row => row.Summary.ToDomain(row.OpenAlert))];
    }

    public void Add(AbsenceSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var entity = new StudentAttendanceSummary();
        summary.CopyTo(entity);
        db.StudentAttendanceSummaries.Add(entity);
        AddRaisedAlert(summary);
    }

    public void Update(AbsenceSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var entity = db.StudentAttendanceSummaries.Local.FirstOrDefault(s => s.SummaryId == summary.Id)
            ?? throw new InvalidOperationException($"Absence summary {summary.Id} was not loaded in this unit of work.");
        summary.CopyTo(entity);
        AddRaisedAlert(summary);
    }

    private void AddRaisedAlert(AbsenceSummary summary)
    {
        if (summary.RaisedAlert is { } alert)
        {
            db.StudentAlerts.Add(alert.ToEntity(summary));
        }
    }
}
