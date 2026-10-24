using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.AbsenceSummaries;

public interface IAbsenceSummaryRepository
{
    /// <summary>The students' summaries at the school for the year, each with its open alert, in one query.</summary>
    Task<IReadOnlyList<AbsenceSummary>> GetManyAsync(
        int schoolId, SchoolYear schoolYear, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken);

    /// <summary>Adds a new summary, and its <see cref="AbsenceSummary.RaisedAlert"/> if there is one.</summary>
    void Add(AbsenceSummary summary);

    /// <summary>Updates a stored summary, and adds its <see cref="AbsenceSummary.RaisedAlert"/> if there is one.</summary>
    void Update(AbsenceSummary summary);
}
