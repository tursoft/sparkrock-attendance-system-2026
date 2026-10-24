using AttendanceSystem.Application.Common;

namespace AttendanceSystem.Application.Students;

internal static class StudentErrors
{
    public static Error NotFound(int studentId) => Error.NotFound("student.not_found", $"Student {studentId} was not found.");

    public static Error SchoolNotFound(int schoolId) => Error.NotFound("school.not_found", $"School {schoolId} was not found.");
}
