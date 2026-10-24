using System.Net;
using System.Net.Http.Json;
using AttendanceSystem.Api.IntegrationTests.Fixtures;
using AttendanceSystem.Infrastructure.Persistence.Entities;

namespace AttendanceSystem.Api.IntegrationTests.StudentAttendance;

public sealed class StudentAbsenceSummaryTests(DatabaseFixture database) : IAsyncDisposable
{
    private const string CurrentYear = "2026-2027";

    private readonly AttendanceApiFactory _factory = new(database);
    private readonly TestData _data = new(database);

    [Theory]
    [InlineData(3, 2, false)]
    [InlineData(3, 3, true)]
    [InlineData(null, 9, false)]
    [InlineData(null, 10, true)]
    public async Task Chronically_absent_when_total_reaches_the_effective_threshold(int? threshold, int total, bool expected)
    {
        var school = await CreateSchoolWithCurrentYearAsync(threshold);
        var student = await _data.CreateStudentAsync(school.SchoolId);
        await AddSummaryAsync(student.StudentId, school.SchoolId, CurrentYear, total);

        var summary = await GetSummaryAsync($"/students/{student.StudentId}/absence-summary");

        Assert.Equal(
            new SummaryResponse(student.StudentId, school.SchoolId, CurrentYear, total, threshold ?? 10, expected),
            summary);
    }

    [Fact]
    public async Task No_summary_means_zero_absences()
    {
        var school = await CreateSchoolWithCurrentYearAsync(threshold: 5);
        var student = await _data.CreateStudentAsync(school.SchoolId);

        var summary = await GetSummaryAsync($"/students/{student.StudentId}/absence-summary");

        Assert.Equal(new SummaryResponse(student.StudentId, school.SchoolId, CurrentYear, 0, 5, false), summary);
    }

    [Fact]
    public async Task School_id_selects_another_schools_summary()
    {
        var previousSchool = await CreateSchoolWithCurrentYearAsync(threshold: 2);
        var school = await CreateSchoolWithCurrentYearAsync(threshold: 10);
        var student = await _data.CreateStudentAsync(school.SchoolId);
        await AddSummaryAsync(student.StudentId, previousSchool.SchoolId, CurrentYear, 2);
        await AddSummaryAsync(student.StudentId, school.SchoolId, CurrentYear, 1);

        var summary = await GetSummaryAsync(
            $"/students/{student.StudentId}/absence-summary?schoolId={previousSchool.SchoolId}&schoolYear={CurrentYear}");

        Assert.Equal(new SummaryResponse(student.StudentId, previousSchool.SchoolId, CurrentYear, 2, 2, true), summary);
    }

    [Fact]
    public async Task Unknown_student_or_school_returns_404()
    {
        var school = await CreateSchoolWithCurrentYearAsync(threshold: null);
        var student = await _data.CreateStudentAsync(school.SchoolId);
        var client = _factory.CreateClient();

        var unknownStudent = await client.GetAsync("/students/999999999/absence-summary", TestContext.Current.CancellationToken);
        var unknownSchool = await client.GetAsync(
            $"/students/{student.StudentId}/absence-summary?schoolId=999999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, unknownStudent.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownSchool.StatusCode);
    }

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    private async Task<School> CreateSchoolWithCurrentYearAsync(int? threshold)
    {
        var school = await _data.CreateSchoolAsync(absenceAlertThreshold: threshold);
        await _data.AddTermAsync(school.SchoolId, CurrentYear, new(2026, 9, 1), new(2027, 6, 30));
        return school;
    }

    private Task<StudentAttendanceSummary> AddSummaryAsync(int studentId, int schoolId, string schoolYear, int total) =>
        _data.InsertAsync(new StudentAttendanceSummary
        {
            StudentId = studentId,
            SchoolId = schoolId,
            SchoolYear = schoolYear,
            TotalAbsences = total,
        });

    private async Task<SummaryResponse?> GetSummaryAsync(string url) =>
        await _factory.CreateClient().GetFromJsonAsync<SummaryResponse>(url, TestContext.Current.CancellationToken);

    private sealed record SummaryResponse(
        int StudentId, int SchoolId, string SchoolYear, int TotalAbsences, int Threshold, bool IsChronicallyAbsent);
}
