using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Application.Students.AbsenceSummary;

public interface IAbsenceSummaryQuery
{
    /// <summary>The stored total for the student at the school in the year, or 0 when there is no summary (#47, #56).</summary>
    Task<int> GetTotalAbsencesAsync(int studentId, int schoolId, SchoolYear schoolYear, CancellationToken cancellationToken);
}
