namespace AttendanceSystem.Domain.Attendance;

public interface IAttendanceRecordRepository
{
    /// <summary>The students' existing records on the date, whichever school saved them, in one query.</summary>
    Task<IReadOnlyList<AttendanceRecord>> GetForDateAsync(
        IReadOnlyCollection<int> studentIds, DateOnly attendDate, CancellationToken cancellationToken);

    /// <summary>
    /// Absences per student at the school from <paramref name="startDate"/> to <paramref name="endDate"/> inclusive,
    /// leaving out <paramref name="excludedDate"/>, in one grouped query. Excused absences count.
    /// Students with no absences are left out of the result.
    /// </summary>
    Task<IReadOnlyDictionary<int, int>> CountAbsencesAsync(
        int schoolId,
        IReadOnlyCollection<int> studentIds,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly excludedDate,
        CancellationToken cancellationToken);

    void Add(AttendanceRecord record);

    void Update(AttendanceRecord record);
}
