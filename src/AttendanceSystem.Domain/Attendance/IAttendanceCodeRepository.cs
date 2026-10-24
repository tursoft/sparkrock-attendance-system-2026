namespace AttendanceSystem.Domain.Attendance;

public interface IAttendanceCodeRepository
{
    /// <summary>The codes that exist among the values, active or not, in one query. Unknown values are left out.</summary>
    Task<IReadOnlyList<AttendanceCode>> GetManyAsync(IReadOnlyCollection<string> codeValues, CancellationToken cancellationToken);
}
