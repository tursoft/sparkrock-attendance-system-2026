using Microsoft.AspNetCore.HttpLogging;

namespace AttendanceSystem.Api.Hosting;

internal static class LoggingExtensions
{
    /// <summary>JSON event stream on stdout; scopes carry the request's TraceId and SpanId.</summary>
    public static ILoggingBuilder AddJsonStdoutLogging(this ILoggingBuilder logging) =>
        logging.ClearProviders().AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

    /// <summary>One entry per request. Headers, query strings and bodies are left out because they may hold personal data.</summary>
    public static IServiceCollection AddRequestLogging(this IServiceCollection services) =>
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });
}
