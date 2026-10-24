using System.Net;
using System.Net.Http.Json;
using AttendanceSystem.Api.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Api.IntegrationTests.StudentAttendance;

public sealed class StudentAttendanceHistoryTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly AttendanceApiFactory _factory = new(database);
    private readonly TestData _data = new(database);

    [Fact]
    public async Task Defaults_to_the_current_year_newest_first_across_schools()
    {
        var previousSchool = await _data.CreateSchoolAsync();
        await _data.AddTermAsync(previousSchool.SchoolId, "2026-2027", new(2026, 9, 1), new(2026, 12, 20), "Fall");
        var school = await _data.CreateSchoolAsync();
        await _data.AddTermAsync(school.SchoolId, "2025-2026", new(2025, 9, 1), new(2026, 6, 30));
        var fall = await _data.AddTermAsync(school.SchoolId, "2026-2027", new(2026, 9, 15), new(2026, 12, 20), "Fall term");
        var student = await _data.CreateStudentAsync(school.SchoolId);

        await AddRecordAsync(student.StudentId, previousSchool.SchoolId, new(2026, 9, 3), "A");
        await AddRecordAsync(student.StudentId, school.SchoolId, new(2026, 10, 1), "E", fall.TermId, notes: "Doctor");
        await AddRecordAsync(student.StudentId, school.SchoolId, new(2026, 10, 2), "L", fall.TermId, minutesLate: 15);
        await AddRecordAsync(student.StudentId, school.SchoolId, new(2026, 3, 2), "A"); // previous year

        var client = _factory.CreateClient();
        var page1 = await client.GetFromJsonAsync<HistoryResponse>(
            $"/students/{student.StudentId}/attendance?pageSize=2", TestContext.Current.CancellationToken);
        var page2 = await client.GetFromJsonAsync<HistoryResponse>(
            $"/students/{student.StudentId}/attendance?page=2&pageSize=2", TestContext.Current.CancellationToken);

        Assert.NotNull(page1);
        Assert.Equal(("2026-2027", 1, 2, 3), (page1.SchoolYear, page1.Page, page1.PageSize, page1.TotalCount));
        Assert.Equal(
            [
                new(new(2026, 10, 2), school.SchoolId, "L", "Late", false, false, 15, null, "Fall term"),
                new(new(2026, 10, 1), school.SchoolId, "E", "Excused absence", true, true, 0, "Doctor", "Fall term"),
            ],
            page1.Items);

        Assert.NotNull(page2);
        var fromPreviousSchool = Assert.Single(page2.Items);
        Assert.Equal((new DateOnly(2026, 9, 3), previousSchool.SchoolId, "A"), (fromPreviousSchool.Date, fromPreviousSchool.SchoolId, fromPreviousSchool.Code));
    }

    [Fact]
    public async Task Unknown_student_returns_404()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/students/999999999/attendance", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal(404, problem?.Status);
    }

    [Fact]
    public async Task Malformed_school_year_returns_400()
    {
        var school = await _data.CreateSchoolAsync();
        var student = await _data.CreateStudentAsync(school.SchoolId);

        var response = await _factory.CreateClient().GetAsync(
            $"/students/{student.StudentId}/attendance?schoolYear=2025-2027", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Contains("schoolYear", problem!.Errors.Keys);
    }

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    private Task<Infrastructure.Persistence.Entities.StudentAttendance> AddRecordAsync(
        int studentId, int schoolId, DateOnly date, string code, int? termId = null, int minutesLate = 0, string? notes = null) =>
        _data.InsertAsync(new Infrastructure.Persistence.Entities.StudentAttendance
        {
            StudentId = studentId,
            SchoolId = schoolId,
            AttendDate = date,
            TermId = termId,
            AttendCode = code,
            IsAbsent = code is "A" or "E",
            IsExcused = code is "E",
            MinutesLate = minutesLate,
            Notes = notes,
            CreatedBy = "test",
        });

    private sealed record HistoryResponse(string SchoolYear, int Page, int PageSize, int TotalCount, IReadOnlyList<HistoryItem> Items);

    private sealed record HistoryItem(
        DateOnly Date, int SchoolId, string Code, string CodeDescription, bool IsAbsent, bool IsExcused, int MinutesLate, string? Notes, string? TermName);
}
