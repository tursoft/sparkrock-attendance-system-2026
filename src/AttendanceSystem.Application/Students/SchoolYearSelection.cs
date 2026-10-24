using AttendanceSystem.Application.Common;
using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Application.Students;

/// <summary>The school year a student query reports on: the requested one, or the school's current year (#32, #55).</summary>
internal static class SchoolYearSelection
{
    public const string Field = "schoolYear";

    public static Result<SchoolYear> Parse(string? requested) =>
        SchoolYear.TryParse(requested, out var schoolYear)
            ? schoolYear
            : Error.Validation(Field, "The school year must be YYYY-YYYY with consecutive years, such as 2025-2026.");

    public static Result<SchoolYear> Current(School school, TimeProvider clock) =>
        school.CurrentSchoolYear(school.Today(clock)) is { } current
            ? current.SchoolYear
            : Error.Validation(Field, "The school has no school year that has started yet; specify schoolYear.");
}
