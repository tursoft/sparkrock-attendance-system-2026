using AttendanceSystem.Domain.Students;
using Riok.Mapperly.Abstractions;
using StudentEntity = AttendanceSystem.Infrastructure.Persistence.Entities.Student;

namespace AttendanceSystem.Infrastructure.Repositories;

/// <summary>Builds the <see cref="Student"/> aggregate through its constructor.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class StudentMapper
{
    [MapProperty(nameof(StudentEntity.StudentId), "id")]
    [MapProperty(nameof(StudentEntity.Active), "isActive")]
    public static partial Student ToDomain(StudentEntity student);
}
