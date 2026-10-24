using AttendanceSystem.Domain.Attendance;
using AttendanceSystem.Infrastructure.Mapping;
using AttendanceSystem.Infrastructure.Persistence;
using AttendanceSystem.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories;

internal sealed class AttendanceRecordRepository(AttendanceDbContext db) : IAttendanceRecordRepository
{
    /// <remarks>Tracked, so <see cref="Update"/> writes back to the same row and its row version guards the race (#41).</remarks>
    public async Task<IReadOnlyList<AttendanceRecord>> GetForDateAsync(
        IReadOnlyCollection<int> studentIds, DateOnly attendDate, CancellationToken cancellationToken)
    {
        var rows = await db.StudentAttendances
            .Where(a => a.AttendDate == attendDate && studentIds.Contains(a.StudentId))
            .ToListAsync(cancellationToken);
        return [.. rows.Select(a => a.ToDomain())];
    }

    public async Task<IReadOnlyDictionary<int, int>> CountAbsencesAsync(
        int schoolId,
        IReadOnlyCollection<int> studentIds,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly excludedDate,
        CancellationToken cancellationToken) =>
        await db.StudentAttendances
            .AsNoTracking()
            .Where(a => a.SchoolId == schoolId
                && studentIds.Contains(a.StudentId)
                && a.IsAbsent
                && a.AttendDate >= startDate
                && a.AttendDate <= endDate
                && a.AttendDate != excludedDate)
            .GroupBy(a => a.StudentId)
            .Select(g => new { StudentId = g.Key, Absences = g.Count() })
            .ToDictionaryAsync(g => g.StudentId, g => g.Absences, cancellationToken);

    public void Add(AttendanceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        var entity = new StudentAttendance();
        record.CopyTo(entity);
        db.StudentAttendances.Add(entity);
    }

    public void Update(AttendanceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        var entity = db.StudentAttendances.Local.FirstOrDefault(a => a.AttendanceId == record.Id)
            ?? throw new InvalidOperationException($"Attendance record {record.Id} was not loaded in this unit of work.");
        record.CopyTo(entity);
    }
}
