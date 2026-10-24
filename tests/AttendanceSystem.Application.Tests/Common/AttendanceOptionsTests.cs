using AttendanceSystem.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Application.Tests.Common;

public class AttendanceOptionsTests
{
    [Fact]
    public void Audit_user_is_bound_from_the_attendance_section()
    {
        using var provider = BuildProvider(auditUser: "attendance-api");

        provider.GetRequiredService<IStartupValidator>().Validate();

        Assert.Equal("attendance-api", provider.GetRequiredService<IOptions<AttendanceOptions>>().Value.AuditUser);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_audit_user_fails_startup_validation(string? auditUser)
    {
        using var provider = BuildProvider(auditUser);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void An_audit_user_over_one_hundred_characters_fails_startup_validation()
    {
        using var provider = BuildProvider(new string('u', 101));

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void An_audit_user_of_exactly_one_hundred_characters_is_accepted()
    {
        using var provider = BuildProvider(new string('u', 100));

        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    private static ServiceProvider BuildProvider(string? auditUser)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Attendance:AuditUser"] = auditUser })
            .Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddApplication()
            .BuildServiceProvider();
    }
}
