namespace AttendanceSystem.Api.ErrorHandling;

internal static class ErrorHandlingServiceCollectionExtensions
{
    /// <summary>ProblemDetails for every error response, each carrying the request's <c>traceId</c>, plus the global exception handler.</summary>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomization.Apply);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
