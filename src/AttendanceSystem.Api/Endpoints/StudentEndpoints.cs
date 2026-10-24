using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Application.Common;
using AttendanceSystem.Application.Students.AbsenceSummary;
using AttendanceSystem.Application.Students.AttendanceHistory;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Api.Endpoints;

public static class StudentEndpoints
{
    // Parameter annotations document the contract in OpenAPI; the application layer enforces it.
    private const string SchoolYearPattern = @"^\d{4}-\d{4}$";

    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var students = app.MapGroup("/students").WithTags("Students");

        students.MapGet("/{studentId:int}/attendance", GetAttendanceHistoryAsync)
            .WithName("GetStudentAttendanceHistory")
            .WithSummary("A student's attendance history for one school year, newest first, across all schools.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        students.MapGet("/{studentId:int}/absence-summary", GetAbsenceSummaryAsync)
            .WithName("GetStudentAbsenceSummary")
            .WithSummary("A student's total absences and chronic absenteeism status at one school for one school year.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<Results<Ok<AttendanceHistoryDto>, ProblemHttpResult>> GetAttendanceHistoryAsync(
        int studentId,
        [FromQuery, RegularExpression(SchoolYearPattern)] string? schoolYear,
        [FromQuery, Range(1, int.MaxValue), DefaultValue(PageRequest.DefaultPage)] int? page,
        [FromQuery, Range(1, PageRequest.MaxPageSize), DefaultValue(PageRequest.DefaultPageSize)] int? pageSize,
        GetStudentAttendanceHistoryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetStudentAttendanceHistory(studentId, schoolYear, page, pageSize), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<Results<Ok<AbsenceSummaryDto>, ProblemHttpResult>> GetAbsenceSummaryAsync(
        int studentId,
        [FromQuery, RegularExpression(SchoolYearPattern)] string? schoolYear,
        [FromQuery] int? schoolId,
        GetStudentAbsenceSummaryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetStudentAbsenceSummary(studentId, schoolYear, schoolId), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }
}
