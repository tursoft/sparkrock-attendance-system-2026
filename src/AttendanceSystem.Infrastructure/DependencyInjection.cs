using AttendanceSystem.Application.Students.AbsenceSummary;
using AttendanceSystem.Application.Students.AttendanceHistory;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Domain.Students;
using AttendanceSystem.Infrastructure.Persistence;
using AttendanceSystem.Infrastructure.Queries;
using AttendanceSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "Attendance";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"The connection string 'ConnectionStrings:{ConnectionStringName}' is missing. " +
                $"Set it with user-secrets locally or the 'ConnectionStrings__{ConnectionStringName}' environment variable.");
        }

        services.AddDbContext<AttendanceDbContext>(options => options.UseSqlServer(connectionString));
        services.AddHealthChecks().AddDbContextCheck<AttendanceDbContext>();

        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAttendanceHistoryQuery, AttendanceHistoryQuery>();
        services.AddScoped<IAbsenceSummaryQuery, AbsenceSummaryQuery>();

        return services;
    }
}
