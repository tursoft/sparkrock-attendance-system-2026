using AttendanceSystem.Application.Common;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Domain.Students;

namespace AttendanceSystem.Application.Students.AbsenceSummary;

public sealed record GetStudentAbsenceSummary(int StudentId, string? SchoolYear, int? SchoolId);

public sealed record AbsenceSummaryDto(
    int StudentId,
    int SchoolId,
    string SchoolYear,
    int TotalAbsences,
    int Threshold,
    bool IsChronicallyAbsent);

/// <summary>UC3: a student's chronic absenteeism status at one school for one school year.</summary>
public sealed class GetStudentAbsenceSummaryHandler(
    IStudentRepository students,
    ISchoolRepository schools,
    IAbsenceSummaryQuery summaries,
    TimeProvider clock)
{
    public async Task<Result<AbsenceSummaryDto>> HandleAsync(GetStudentAbsenceSummary query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var student = await students.GetAsync(query.StudentId, cancellationToken);
        if (student is null)
        {
            return StudentErrors.NotFound(query.StudentId);
        }

        var schoolId = query.SchoolId ?? student.SchoolId;
        var school = await schools.GetAsync(schoolId, cancellationToken);
        if (school is null)
        {
            return StudentErrors.SchoolNotFound(schoolId);
        }

        var schoolYear = query.SchoolYear is null
            ? SchoolYearSelection.Current(school, clock)
            : SchoolYearSelection.Parse(query.SchoolYear);
        if (schoolYear.IsFailure)
        {
            return schoolYear.Error;
        }

        var total = await summaries.GetTotalAbsencesAsync(student.Id, school.Id, schoolYear.Value, cancellationToken);
        return new AbsenceSummaryDto(
            student.Id, school.Id, schoolYear.Value.ToString(), total, school.AbsenceAlertThreshold, total >= school.AbsenceAlertThreshold);
    }
}
