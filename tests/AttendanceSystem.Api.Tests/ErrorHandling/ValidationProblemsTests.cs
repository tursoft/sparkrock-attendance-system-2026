using AttendanceSystem.Api.ErrorHandling;
using Microsoft.AspNetCore.Http;

namespace AttendanceSystem.Api.Tests.ErrorHandling;

public sealed class ValidationProblemsTests
{
    [Fact]
    public void Creates_a_400_validation_problem_with_the_validation_code()
    {
        var result = ValidationProblems.Create(new Dictionary<string, string[]> { ["date"] = ["Bad date."] });

        var problem = Assert.IsType<HttpValidationProblemDetails>(result.ProblemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("One or more validation errors occurred.", problem.Title);
        Assert.Equal("validation", problem.Extensions["code"]);
        Assert.Equal(["Bad date."], problem.Errors["date"]);
    }
}
