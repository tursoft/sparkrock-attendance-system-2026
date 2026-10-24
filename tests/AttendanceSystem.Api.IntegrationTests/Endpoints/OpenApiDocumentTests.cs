using System.Net;
using AttendanceSystem.Api.IntegrationTests.Fixtures;

namespace AttendanceSystem.Api.IntegrationTests.Endpoints;

public sealed class OpenApiDocumentTests(DatabaseFixture database)
{
    [Fact]
    public async Task OpenApi_document_is_served()
    {
        await using var factory = new AttendanceApiFactory(database);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"openapi\"", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }
}
