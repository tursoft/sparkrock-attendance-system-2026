namespace AttendanceSystem.Domain.Students;

public interface IStudentRepository
{
    Task<Student?> GetAsync(int studentId, CancellationToken cancellationToken);

    /// <summary>The students that exist among the ids, in one query. Unknown ids are left out.</summary>
    Task<IReadOnlyList<Student>> GetManyAsync(IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken);
}
