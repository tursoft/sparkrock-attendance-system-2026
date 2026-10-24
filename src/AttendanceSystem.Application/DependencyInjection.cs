using AttendanceSystem.Application.Common;
using AttendanceSystem.Application.Students.AbsenceSummary;
using AttendanceSystem.Application.Students.AttendanceHistory;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<AttendanceOptions>()
            .BindConfiguration(AttendanceOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<GetStudentAttendanceHistoryHandler>();
        services.AddScoped<GetStudentAbsenceSummaryHandler>();

        return services;
    }
}
