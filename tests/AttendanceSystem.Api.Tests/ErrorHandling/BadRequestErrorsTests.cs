using System.Text.Json;
using AttendanceSystem.Api.ErrorHandling;
using Microsoft.AspNetCore.Http;

namespace AttendanceSystem.Api.Tests.ErrorHandling;

public sealed class BadRequestErrorsTests
{
    [Fact]
    public void A_json_value_of_the_wrong_type_is_keyed_by_its_path()
    {
        var exception = new BadHttpRequestException("Failed to read parameter.", new JsonException("Bad value.", "$.entries[0].studentId", 1, 20));

        var errors = BadRequestErrors.FieldErrors(exception);

        Assert.Equal(["entries[0].studentId"], errors!.Keys);
    }

    [Fact]
    public void An_unparsable_route_or_query_value_is_keyed_by_its_parameter()
    {
        var exception = new BadHttpRequestException("""Failed to bind parameter "Nullable<int> schoolId" from "abc".""");

        var errors = BadRequestErrors.FieldErrors(exception);

        Assert.Equal(["schoolId"], errors!.Keys);
    }

    [Theory]
    [InlineData("$")]
    [InlineData(null)]
    public void A_json_error_without_a_field_path_has_no_field_errors(string? path)
    {
        var exception = new BadHttpRequestException("Failed to read parameter.", new JsonException("Bad JSON.", path, 1, 1));

        Assert.Null(BadRequestErrors.FieldErrors(exception));
    }

    [Fact]
    public void Any_other_bad_request_has_no_field_errors()
    {
        Assert.Null(BadRequestErrors.FieldErrors(new BadHttpRequestException("Unexpected end of request content.")));
    }
}
