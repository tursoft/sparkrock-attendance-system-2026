using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AttendanceSystem.Api.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Api.IntegrationTests.Endpoints;

public sealed class ValidationFilterTests
{
    public sealed record Entry([property: Range(1, int.MaxValue)] int StudentId, [property: Required] string? Code);

    public sealed record Request([property: Required] string? ClassName, [property: Required] IReadOnlyList<Entry>? Entries);

    private static readonly ValidationFilter<Request> Filter =
        new(Options.Create(new JsonOptions { SerializerOptions = { PropertyNamingPolicy = JsonNamingPolicy.CamelCase } }));

    private static ValueTask<object?> Run(Request request) =>
        Filter.InvokeAsync(
            EndpointFilterInvocationContext.Create(new DefaultHttpContext(), request),
            _ => ValueTask.FromResult<object?>("handler"));

    [Fact]
    public async Task Valid_request_reaches_the_handler()
    {
        var result = await Run(new Request("1A", [new Entry(1, "P")]));

        Assert.Equal("handler", result);
    }

    [Fact]
    public async Task Invalid_request_returns_errors_keyed_by_json_path_including_collection_items()
    {
        var result = await Run(new Request(null, [new Entry(1, "P"), new Entry(0, null)]));

        var problem = Assert.IsType<ValidationProblem>(result);
        Assert.Equal(
            ["className", "entries[1].code", "entries[1].studentId"],
            problem.ProblemDetails.Errors.Keys.Order(StringComparer.Ordinal));
    }
}
