using AttendanceSystem.Application.Common;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Domain.Students;

namespace AttendanceSystem.Application.Students.AttendanceHistory;

public sealed record GetStudentAttendanceHistory(int StudentId, string? SchoolYear, int? Page, int? PageSize);

/// <summary>UC2: a student's attendance history for one school year.</summary>
public sealed class GetStudentAttendanceHistoryHandler(
    IStudentRepository students,
    ISchoolRepository schools,
    IAttendanceHistoryQuery history,
    TimeProvider clock)
{
    public async Task<Result<AttendanceHistoryDto>> HandleAsync(GetStudentAttendanceHistory query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = PageRequest.Create(query.Page, query.PageSize);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var student = await students.GetAsync(query.StudentId, cancellationToken);
        if (student is null)
        {
            return StudentErrors.NotFound(query.StudentId);
        }

        var schoolYear = await ResolveSchoolYearAsync(query.SchoolYear, student, cancellationToken);
        if (schoolYear.IsFailure)
        {
            return schoolYear.Error;
        }

        var items = await history.GetPageAsync(student.Id, schoolYear.Value, page.Value, cancellationToken);
        return new AttendanceHistoryDto(schoolYear.Value.ToString(), items.Page, items.PageSize, items.TotalCount, items.Items);
    }

    private async Task<Result<SchoolYear>> ResolveSchoolYearAsync(string? requested, Student student, CancellationToken cancellationToken)
    {
        if (requested is not null)
        {
            return SchoolYearSelection.Parse(requested);
        }

        var school = await schools.GetAsync(student.SchoolId, cancellationToken)
            ?? throw new InvalidOperationException($"Student {student.Id} belongs to missing school {student.SchoolId}.");
        return SchoolYearSelection.Current(school, clock);
    }
}
