using AttendanceSystem.Application.Common;

namespace AttendanceSystem.Application.Tests.Common;

public class ResultTests
{
    private static readonly Error NotFound = Error.NotFound("student.not_found", "Student 5 was not found.");

    [Fact]
    public void Success_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_carries_its_error()
    {
        var result = Result.Failure(NotFound);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Same(NotFound, result.Error);
    }

    [Fact]
    public void An_error_converts_to_a_failed_result()
    {
        Result result = NotFound;

        Assert.Same(NotFound, result.Error);
    }

    [Fact]
    public void Failure_needs_an_error()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
    }

    [Fact]
    public void Typed_success_carries_its_value()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void A_value_converts_to_a_successful_typed_result()
    {
        Result<string> result = "done";

        Assert.True(result.IsSuccess);
        Assert.Equal("done", result.Value);
    }

    [Fact]
    public void An_error_converts_to_a_failed_typed_result()
    {
        Result<int> result = NotFound;

        Assert.True(result.IsFailure);
        Assert.Same(NotFound, result.Error);
    }

    [Fact]
    public void Reading_the_value_of_a_failed_result_throws()
    {
        Result<int> result = NotFound;

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Typed_failure_needs_an_error()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure<int>(null!));
    }
}
