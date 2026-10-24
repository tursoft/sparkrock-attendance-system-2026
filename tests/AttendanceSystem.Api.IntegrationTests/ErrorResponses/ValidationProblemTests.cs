using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AttendanceSystem.Api.IntegrationTests.Fixtures;

namespace AttendanceSystem.Api.IntegrationTests.ErrorResponses;

/// <summary>Every 400 has one shape: problem+json with <c>errors</c> keyed by request field, <c>code</c> "validation" and a <c>traceId</c>.</summary>
public sealed class ValidationProblemTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly AttendanceApiFactory _factory = new(database);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    [Theory]
    [InlineData("""{"entries":[null]}""", "entries[0]")]
    [InlineData("""{"entries":[{"studentId":1,"code":"P"},null]}""", "entries[1]")]
    public async Task A_null_entry_is_a_validation_error(string body, string expectedKey)
    {
        var response = await PutJsonAsync("/schools/1/attendance/2026-10-20", body);

        await AssertValidationProblemAsync(response, expectedKey);
    }

    [Theory]
    [InlineData("2026-13-01")]
    [InlineData("abc")]
    public async Task A_malformed_date_is_a_validation_problem_with_trace_id(string date)
    {
        var response = await PutJsonAsync($"/schools/1/attendance/{date}", """{"entries":[{"studentId":1,"code":"P"}]}""");

        await AssertValidationProblemAsync(response, "date");
    }

    [Fact]
    public async Task A_field_rule_violation_has_the_validation_code()
    {
        var response = await PutJsonAsync("/schools/1/attendance/2026-10-20", """{"entries":[{"studentId":0,"code":"P"}]}""");

        await AssertValidationProblemAsync(response, "entries[0].studentId");
    }

    [Theory]
    [InlineData("""{"entries":[{"studentId":2147483648,"code":"P"}]}""", "entries[0].studentId")]
    [InlineData("""{"entries":[{"studentId":1,"code":"P","minutesLate":1.5}]}""", "entries[0].minutesLate")]
    [InlineData("""{"entries":[{"studentId":1,"code":5}]}""", "entries[0].code")]
    public async Task A_body_value_of_the_wrong_json_type_names_its_field(string body, string expectedKey)
    {
        var response = await PutJsonAsync("/schools/1/attendance/2026-10-20", body);

        await AssertValidationProblemAsync(response, expectedKey);
    }

    [Theory]
    [InlineData("/students/1/absence-summary?schoolId=abc", "schoolId")]
    [InlineData("/students/1/attendance?page=abc", "page")]
    public async Task A_query_value_that_cannot_be_parsed_names_its_parameter(string url, string expectedKey)
    {
        var response = await _factory.CreateClient().GetAsync(new Uri(url, UriKind.Relative), CancellationToken);

        await AssertValidationProblemAsync(response, expectedKey);
    }

    private async Task<HttpResponseMessage> PutJsonAsync(string url, string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _factory.CreateClient().PutAsync(new Uri(url, UriKind.Relative), content, CancellationToken);
    }

    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedKey)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(CancellationToken);
        Assert.True(problem.GetProperty("errors").TryGetProperty(expectedKey, out _), $"Expected an error keyed '{expectedKey}': {problem}");
        Assert.Equal("validation", problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }
}
