using AttendanceSystem.Application.Common;
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

        return services;
    }
}
