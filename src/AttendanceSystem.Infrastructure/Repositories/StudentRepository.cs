using AttendanceSystem.Domain.Students;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories;

public sealed class StudentRepository(AttendanceDbContext db) : IStudentRepository
{
    public async Task<Student?> GetAsync(int studentId, CancellationToken cancellationToken)
    {
        var student = await db.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        return student is null ? null : StudentMapper.ToDomain(student);
    }

    public async Task<IReadOnlyList<Student>> GetManyAsync(IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken)
    {
        var students = await db.Students
            .AsNoTracking()
            .Where(s => studentIds.Contains(s.StudentId))
            .ToListAsync(cancellationToken);

        return [.. students.Select(StudentMapper.ToDomain)];
    }
}
