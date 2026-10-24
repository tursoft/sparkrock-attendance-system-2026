namespace AttendanceSystem.Api.Endpoints;

public static class ApiEndpoints
{
    /// <summary>Maps every resource group. Each resource file adds one line here, such as <c>app.MapSchoolAttendanceEndpoints();</c>.</summary>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapStudentEndpoints();
        return app;
    }
}
