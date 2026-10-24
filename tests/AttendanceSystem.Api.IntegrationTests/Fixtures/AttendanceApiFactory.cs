using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace AttendanceSystem.Api.IntegrationTests.Fixtures;

/// <summary>The real API pipeline against the test database, with a controllable clock.</summary>
public sealed class AttendanceApiFactory(DatabaseFixture database) : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset DefaultUtcNow = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The clock the API sees; tests move it with <c>SetUtcNow</c> or <c>Advance</c>.</summary>
    public FakeTimeProvider TimeProvider { get; } = new(DefaultUtcNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // UseSetting, unlike ConfigureAppConfiguration, is visible to Program while it registers services.
        builder.UseSetting("ConnectionStrings:Attendance", database.ConnectionString);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(TimeProvider);
        });
    }
}
