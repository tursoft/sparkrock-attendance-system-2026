using AttendanceSystem.Application.Common;

namespace AttendanceSystem.Application.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void NotFound_carries_code_message_and_kind()
    {
        var error = Error.NotFound("school.not_found", "School 3 was not found.");

        Assert.Equal("school.not_found", error.Code);
        Assert.Equal("School 3 was not found.", error.Message);
        Assert.Equal(ErrorKind.NotFound, error.Kind);
        Assert.Empty(error.ValidationErrors);
    }

    [Fact]
    public void Conflict_carries_code_message_and_kind()
    {
        var error = Error.Conflict("attendance.concurrency", "Changed by another request.");

        Assert.Equal("attendance.concurrency", error.Code);
        Assert.Equal("Changed by another request.", error.Message);
        Assert.Equal(ErrorKind.Conflict, error.Kind);
        Assert.Empty(error.ValidationErrors);
    }

    [Fact]
    public void Validation_for_one_field_keys_the_message_by_that_field()
    {
        var error = Error.Validation("date", "The date is in the future.");

        Assert.Equal(ErrorKind.Validation, error.Kind);
        Assert.Equal(Error.ValidationCode, error.Code);
        var (field, messages) = Assert.Single(error.ValidationErrors);
        Assert.Equal("date", field);
        Assert.Equal(["The date is in the future."], messages);
    }

    [Fact]
    public void Validation_for_many_fields_keeps_every_field()
    {
        var error = Error.Validation(new Dictionary<string, string[]>
        {
            ["entries[0].studentId"] = ["Student 7 does not belong to the school."],
            ["entries[2].code"] = ["Code X is unknown.", "Code X is inactive."],
        });

        Assert.Equal(ErrorKind.Validation, error.Kind);
        Assert.Equal(2, error.ValidationErrors.Count);
        Assert.Equal(["Code X is unknown.", "Code X is inactive."], error.ValidationErrors["entries[2].code"]);
    }

    [Fact]
    public void Validation_copies_the_errors_so_later_changes_do_not_leak_in()
    {
        var errors = new Dictionary<string, string[]> { ["page"] = ["Too small."] };
        var error = Error.Validation(errors);

        errors["pageSize"] = ["Too big."];

        Assert.Single(error.ValidationErrors);
    }

    [Fact]
    public void Validation_without_any_field_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Error.Validation(new Dictionary<string, string[]>()));
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData(" ", "message")]
    [InlineData("code", "")]
    public void Errors_need_a_code_and_a_message(string code, string message)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.NotFound(code, message));
        Assert.ThrowsAny<ArgumentException>(() => Error.Conflict(code, message));
    }
}
