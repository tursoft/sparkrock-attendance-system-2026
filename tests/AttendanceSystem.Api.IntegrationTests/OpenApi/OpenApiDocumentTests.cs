using System.Net.Http.Json;
using System.Text.Json;
using AttendanceSystem.Api.IntegrationTests.Fixtures;

namespace AttendanceSystem.Api.IntegrationTests.OpenApi;

/// <summary>The OpenAPI document states the contract's constraints and every response an endpoint can return (plan §5).</summary>
public sealed class OpenApiDocumentTests(DatabaseFixture database) : IAsyncDisposable
{
    private const string SaveAttendance = "/schools/{schoolId}/attendance/{date}";
    private const string History = "/students/{studentId}/attendance";
    private const string Summary = "/students/{studentId}/absence-summary";

    private readonly AttendanceApiFactory _factory = new(database);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    [Fact]
    public async Task The_date_path_parameter_is_a_date()
    {
        var schema = ParameterSchema(await GetDocumentAsync(), SaveAttendance, "put", "date");

        Assert.Equal("date", schema.GetProperty("format").GetString());
        Assert.Equal(@"^\d{4}-\d{2}-\d{2}$", schema.GetProperty("pattern").GetString());
    }

    [Theory]
    [InlineData(History)]
    [InlineData(Summary)]
    public async Task The_school_year_query_has_its_pattern(string path)
    {
        var schema = ParameterSchema(await GetDocumentAsync(), path, "get", "schoolYear");

        Assert.Equal(@"^\d{4}-\d{4}$", schema.GetProperty("pattern").GetString());
    }

    [Theory]
    [InlineData("page", 1, int.MaxValue, 1)]
    [InlineData("pageSize", 1, 200, 50)]
    public async Task Paging_parameters_have_bounds_and_defaults(string name, int minimum, int maximum, int @default)
    {
        var schema = ParameterSchema(await GetDocumentAsync(), History, "get", name);

        Assert.Equal(minimum, schema.GetProperty("minimum").GetInt32());
        Assert.Equal(maximum, schema.GetProperty("maximum").GetInt32());
        Assert.Equal(@default, schema.GetProperty("default").GetInt32());
    }

    [Theory]
    [InlineData(SaveAttendance, "put", "200,400,404,409,415,500")]
    [InlineData(History, "get", "200,400,404,500")]
    [InlineData(Summary, "get", "200,400,404,500")]
    public async Task Every_status_code_is_documented(string path, string method, string expected)
    {
        var responses = (await GetDocumentAsync()).GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses");

        Assert.Equal(expected.Split(','), responses.EnumerateObject().Select(response => response.Name).Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("SaveDailyAttendanceResponse", "students")]
    [InlineData("SaveDailyAttendanceResponse", "schoolYear")]
    [InlineData("AttendanceHistoryDto", "items")]
    public async Task Response_fields_that_are_always_present_are_required_and_not_nullable(string schemaName, string property)
    {
        var schema = (await GetDocumentAsync()).GetProperty("components").GetProperty("schemas").GetProperty(schemaName);

        Assert.Contains(property, schema.GetProperty("required").EnumerateArray().Select(name => name.GetString()));
        var propertySchema = schema.GetProperty("properties").GetProperty(property);
        Assert.False(propertySchema.TryGetProperty("nullable", out var nullable) && nullable.GetBoolean());
    }

    private async Task<JsonElement> GetDocumentAsync() =>
        await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json", CancellationToken);

    private static JsonElement ParameterSchema(JsonElement document, string path, string method, string name) =>
        document.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("parameters")
            .EnumerateArray().Single(parameter => parameter.GetProperty("name").GetString() == name)
            .GetProperty("schema");
}
