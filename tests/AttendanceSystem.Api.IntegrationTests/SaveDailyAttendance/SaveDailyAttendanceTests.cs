using System.Net;
using System.Net.Http.Json;
using AttendanceSystem.Api.IntegrationTests.Fixtures;
using AttendanceSystem.Application.Attendance.SaveDailyAttendance;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AttendanceSystem.Api.IntegrationTests.SaveDailyAttendance;

public sealed class SaveDailyAttendanceTests(DatabaseFixture database) : IAsyncDisposable
{
    private const string SchoolYear = "2026-2027";
    private static readonly DateOnly TermStart = new(2026, 9, 1);
    private static readonly DateOnly TermEnd = new(2027, 1, 31);

    private readonly AttendanceApiFactory _factory = new(database);
    private readonly TestData _data = new(database);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    [Fact]
    public async Task Saves_records_summaries_and_submission_log()
    {
        var (schoolId, termId) = await CreateSchoolWithTermAsync();
        var absent = await _data.CreateStudentAsync(schoolId);
        var present = await _data.CreateStudentAsync(schoolId);

        var response = await PutAsync(_factory.CreateClient(), schoolId, "2026-10-20",
            Entry(absent.StudentId, "a", notes: "Called in sick"),
            Entry(present.StudentId, "P", minutesLate: 5));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SaveDailyAttendanceResponse>(CancellationToken);
        Assert.NotNull(body);
        Assert.Equal((schoolId, new DateOnly(2026, 10, 20), SchoolYear, 10), (body.SchoolId, body.Date, body.SchoolYear, body.Threshold));
        Assert.Equal(
            [new(absent.StudentId, 1, false, false), new(present.StudentId, 0, false, false)],
            body.Students);

        var rows = await _data.QueryAsync(db => db.StudentAttendances
            .Where(a => a.SchoolId == schoolId)
            .OrderBy(a => a.StudentId)
            .ToListAsync(CancellationToken));
        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal(("A", true, false, 0, "Called in sick", termId), (row.AttendCode, row.IsAbsent, row.IsExcused, row.MinutesLate, row.Notes, row.TermId));
                Assert.Equal(("attendance-api", AttendanceApiFactory.DefaultUtcNow.UtcDateTime), (row.CreatedBy, row.CreatedDate));
                Assert.Null(row.ModifiedDate);
            },
            row => Assert.Equal(("P", false, 5), (row.AttendCode, row.IsAbsent, row.MinutesLate)));

        var summaries = await _data.QueryAsync(db => db.StudentAttendanceSummaries
            .Where(s => s.SchoolId == schoolId)
            .OrderBy(s => s.StudentId)
            .Select(s => new { s.StudentId, s.SchoolYear, s.TotalAbsences })
            .ToListAsync(CancellationToken));
        Assert.Equal(
            [new { absent.StudentId, SchoolYear, TotalAbsences = 1 }, new { present.StudentId, SchoolYear, TotalAbsences = 0 }],
            summaries);

        var log = await _data.QueryAsync(db => db.AttendanceSubmissionLogs.SingleAsync(l => l.SchoolId == schoolId, CancellationToken));
        Assert.Equal((new DateOnly(2026, 10, 20), 2, "attendance-api"), (log.AttendDate, log.RecordCount, log.SubmittedBy));
    }

    [Fact]
    public async Task Reaching_the_threshold_raises_one_alert_and_a_resave_revises_the_record()
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync(absenceAlertThreshold: 2);
        var student = await _data.CreateStudentAsync(schoolId);
        var client = _factory.CreateClient();

        await PutAsync(client, schoolId, "2026-10-19", Entry(student.StudentId, "A"));
        var reached = await PutAsync(client, schoolId, "2026-10-20", Entry(student.StudentId, "A"));
        var resaved = await PutAsync(client, schoolId, "2026-10-20", Entry(student.StudentId, "E", notes: "Doctor's note"));

        var reachedBody = await reached.Content.ReadFromJsonAsync<SaveDailyAttendanceResponse>(CancellationToken);
        Assert.Equal(new StudentAbsenceResult(student.StudentId, 2, true, true), Assert.Single(reachedBody!.Students));
        var resavedBody = await resaved.Content.ReadFromJsonAsync<SaveDailyAttendanceResponse>(CancellationToken);
        Assert.Equal(new StudentAbsenceResult(student.StudentId, 2, true, false), Assert.Single(resavedBody!.Students));

        var alert = await _data.QueryAsync(db => db.StudentAlerts.SingleAsync(a => a.StudentId == student.StudentId, CancellationToken));
        Assert.Equal(("CHRONIC_ABSENCE", SchoolYear, "Student has reached 2 absences this year"), (alert.AlertType, alert.SchoolYear, alert.AlertMessage));

        var record = await _data.QueryAsync(db => db.StudentAttendances
            .SingleAsync(a => a.StudentId == student.StudentId && a.AttendDate == new DateOnly(2026, 10, 20), CancellationToken));
        Assert.Equal(("E", true, true, "Doctor's note"), (record.AttendCode, record.IsAbsent, record.IsExcused, record.Notes));
        Assert.Equal("attendance-api", record.ModifiedBy);
        Assert.NotNull(record.ModifiedDate);
    }

    [Fact]
    public async Task Rejects_foreign_or_unknown_students_and_unknown_codes_and_persists_nothing()
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync();
        var otherSchool = await _data.CreateSchoolAsync();
        var foreignStudent = await _data.CreateStudentAsync(otherSchool.SchoolId);
        var ownStudent = await _data.CreateStudentAsync(schoolId);

        var response = await PutAsync(_factory.CreateClient(), schoolId, "2026-10-20",
            Entry(foreignStudent.StudentId, "A"),
            Entry(ownStudent.StudentId, "ZZZZZ"),
            Entry(int.MaxValue, "P"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(CancellationToken);
        Assert.Equal(
            ["entries[0].studentId", "entries[1].code", "entries[2].studentId"],
            problem!.Errors.Keys.Order(StringComparer.Ordinal));
        await AssertNothingPersistedAsync(schoolId);
    }

    [Theory]
    [InlineData("2026-1-20")]
    [InlineData("20-10-2026")]
    [InlineData("2026-02-30")]
    public async Task Rejects_a_date_not_in_yyyy_MM_dd(string date)
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync();
        var student = await _data.CreateStudentAsync(schoolId);

        var response = await PutAsync(_factory.CreateClient(), schoolId, date, Entry(student.StudentId, "P"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(CancellationToken);
        Assert.Contains("date", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Rejects_an_invalid_entry_at_the_boundary()
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync();
        var student = await _data.CreateStudentAsync(schoolId);

        var response = await PutAsync(_factory.CreateClient(), schoolId, "2026-10-20", Entry(student.StudentId, "P", minutesLate: 601));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertNothingPersistedAsync(schoolId);
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_school()
    {
        var response = await PutAsync(_factory.CreateClient(), int.MaxValue, "2026-10-20", Entry(1, "P"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("2026-08-31")] // before the first term: no school year covers it
    [InlineData("2026-10-25")] // tomorrow in the school's time zone
    public async Task Rejects_a_date_outside_every_school_year_or_in_the_future(string date)
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync();
        var student = await _data.CreateStudentAsync(schoolId);

        var response = await PutAsync(_factory.CreateClient(), schoolId, date, Entry(student.StudentId, "P"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(CancellationToken);
        Assert.Contains("date", problem!.Errors.Keys);
        await AssertNothingPersistedAsync(schoolId);
    }

    [Fact]
    public async Task A_failure_during_the_save_persists_nothing()
    {
        var (schoolId, _) = await CreateSchoolWithTermAsync(absenceAlertThreshold: 1);
        var student = await _data.CreateStudentAsync(schoolId);
        await using var failingFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AttendanceDbContext>>();
            services.AddDbContext<AttendanceDbContext>(options => options
                .UseSqlServer(database.ConnectionString)
                .AddInterceptors(new FailAfterSubmissionLogInsert()));
        }));

        var response = await PutAsync(failingFactory.CreateClient(), schoolId, "2026-10-20", Entry(student.StudentId, "A"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertNothingPersistedAsync(schoolId);
        var alerts = await _data.QueryAsync(db => db.StudentAlerts.CountAsync(a => a.SchoolId == schoolId, CancellationToken));
        Assert.Equal(0, alerts);
    }

    private async Task<(int SchoolId, int TermId)> CreateSchoolWithTermAsync(int? absenceAlertThreshold = null)
    {
        var school = await _data.CreateSchoolAsync(absenceAlertThreshold: absenceAlertThreshold);
        var term = await _data.AddTermAsync(school.SchoolId, SchoolYear, TermStart, TermEnd);
        return (school.SchoolId, term.TermId);
    }

    private async Task AssertNothingPersistedAsync(int schoolId)
    {
        var counts = await _data.QueryAsync(async db => new
        {
            Records = await db.StudentAttendances.CountAsync(a => a.SchoolId == schoolId, CancellationToken),
            Summaries = await db.StudentAttendanceSummaries.CountAsync(s => s.SchoolId == schoolId, CancellationToken),
            Logs = await db.AttendanceSubmissionLogs.CountAsync(l => l.SchoolId == schoolId, CancellationToken),
        });
        Assert.Equal(new { Records = 0, Summaries = 0, Logs = 0 }, counts);
    }

    private static AttendanceEntryRequest Entry(int studentId, string code, int? minutesLate = null, string? notes = null) =>
        new(studentId, code, minutesLate, notes);

    private static Task<HttpResponseMessage> PutAsync(HttpClient client, int schoolId, string date, params AttendanceEntryRequest[] entries) =>
        client.PutAsJsonAsync($"/schools/{schoolId}/attendance/{date}", new SaveDailyAttendanceRequest(entries), CancellationToken);
}
