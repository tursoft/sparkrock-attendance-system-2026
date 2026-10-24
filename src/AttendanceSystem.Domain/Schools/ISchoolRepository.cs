namespace AttendanceSystem.Domain.Schools;

public interface ISchoolRepository
{
    /// <summary>The school with all its terms, or null when it doesn't exist.</summary>
    Task<School?> GetAsync(int schoolId, CancellationToken cancellationToken);
}
