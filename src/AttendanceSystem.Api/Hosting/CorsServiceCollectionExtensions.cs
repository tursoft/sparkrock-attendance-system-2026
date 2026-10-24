using Microsoft.AspNetCore.Cors.Infrastructure;

namespace AttendanceSystem.Api.Hosting;

internal static class CorsServiceCollectionExtensions
{
    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    /// <summary>A default policy that allows only the origins listed in configuration; none by default.</summary>
    public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            var origins = configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [];
            options.AddDefaultPolicy(policy => policy
                .WithOrigins(origins)
                .WithMethods(HttpMethods.Get, HttpMethods.Put)
                .AllowAnyHeader());
        });
        return services;
    }
}
