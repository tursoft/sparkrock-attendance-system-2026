using AttendanceSystem.Application.Students.AbsenceSummary;
using AttendanceSystem.Application.Students.AttendanceHistory;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Api.Endpoints;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var students = app.MapGroup("/students").WithTags("Students");

        students.MapGet("/{studentId:int}/attendance", GetAttendanceHistoryAsync)
            .WithName("GetStudentAttendanceHistory")
            .WithSummary("A student's attendance history for one school year, newest first, across all schools.")
            .ProducesProblem(StatusCodes.Status404NotFound);

        students.MapGet("/{studentId:int}/absence-summary", GetAbsenceSummaryAsync)
            .WithName("GetStudentAbsenceSummary")
            .WithSummary("A student's total absences and chronic absenteeism status at one school for one school year.")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Results<Ok<AttendanceHistoryDto>, ValidationProblem, ProblemHttpResult>> GetAttendanceHistoryAsync(
        int studentId,
        [FromQuery] string? schoolYear,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        GetStudentAttendanceHistoryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetStudentAttendanceHistory(studentId, schoolYear, page, pageSize), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<Results<Ok<AbsenceSummaryDto>, ValidationProblem, ProblemHttpResult>> GetAbsenceSummaryAsync(
        int studentId,
        [FromQuery] string? schoolYear,
        [FromQuery] int? schoolId,
        GetStudentAbsenceSummaryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetStudentAbsenceSummary(studentId, schoolYear, schoolId), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }
}
