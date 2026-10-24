using AttendanceSystem.Api.Endpoints;
using AttendanceSystem.Application.Attendance.SaveDailyAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Api.Tests.Endpoints;

public sealed class ValidationFilterTests
{
    private static readonly object NextResult = new();

    private readonly ValidationFilter<SaveDailyAttendanceRequest> _filter = new(Options.Create(new JsonOptions()));

    [Fact]
    public async Task A_valid_request_reaches_the_handler()
    {
        var result = await InvokeAsync(new SaveDailyAttendanceRequest([new AttendanceEntryRequest(1, "P", null, null)]));

        Assert.Same(NextResult, result);
    }

    [Fact]
    public async Task A_null_entry_is_reported_by_its_index()
    {
        var request = new SaveDailyAttendanceRequest([new AttendanceEntryRequest(1, "P", null, null), null!]);

        var errors = AssertValidationProblem(await InvokeAsync(request));

        Assert.Equal(["entries[1]"], errors.Keys);
    }

    [Fact]
    public async Task Entry_rule_violations_are_keyed_by_json_path()
    {
        var request = new SaveDailyAttendanceRequest([new AttendanceEntryRequest(0, "TOOLONG", null, null)]);

        var errors = AssertValidationProblem(await InvokeAsync(request));

        Assert.Equal(["entries[0].code", "entries[0].studentId"], errors.Keys.Order(StringComparer.Ordinal));
    }

    private ValueTask<object?> InvokeAsync(SaveDailyAttendanceRequest request) =>
        _filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), request),
            _ => ValueTask.FromResult<object?>(NextResult));

    private static IDictionary<string, string[]> AssertValidationProblem(object? result)
    {
        var problem = Assert.IsType<HttpValidationProblemDetails>(Assert.IsType<ProblemHttpResult>(result).ProblemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("validation", problem.Extensions["code"]);
        return problem.Errors;
    }
}
