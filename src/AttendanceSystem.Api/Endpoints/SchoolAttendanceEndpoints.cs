using System.Globalization;
using AttendanceSystem.Application.Attendance.SaveDailyAttendance;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AttendanceSystem.Api.Endpoints;

public static class SchoolAttendanceEndpoints
{
    private const string DateFormat = "yyyy-MM-dd";

    public static IEndpointRouteBuilder MapSchoolAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        var schools = app.MapGroup("/schools").WithTags("Schools");

        schools.MapPut("/{schoolId:int}/attendance/{date}", SaveDailyAttendanceAsync)
            .WithName("SaveDailyAttendance")
            .WithSummary("Save a school's attendance for one date")
            .WithDescription("Records or revises each student's attendance on the date and recalculates their absences for the school year.")
            .WithValidation<SaveDailyAttendanceRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<Results<Ok<SaveDailyAttendanceResponse>, ValidationProblem, ProblemHttpResult>> SaveDailyAttendanceAsync(
        int schoolId,
        string date,
        SaveDailyAttendanceRequest request,
        SaveDailyAttendanceHandler handler,
        CancellationToken cancellationToken)
    {
        // Route binding of DateOnly accepts many formats; the contract allows exactly yyyy-MM-dd.
        if (!DateOnly.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var attendDate))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["date"] = [$"The date must be a valid calendar date in the format {DateFormat}."],
            });
        }

        var result = await handler.HandleAsync(schoolId, attendDate, request, cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }
}
