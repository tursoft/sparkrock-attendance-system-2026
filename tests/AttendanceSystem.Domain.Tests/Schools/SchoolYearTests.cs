using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Tests.Schools;

public class SchoolYearTests
{
    [Theory]
    [InlineData("2025-2026", 2025)]
    [InlineData("1999-2000", 1999)]
    [InlineData("0001-0002", 1)]
    public void TryParse_accepts_consecutive_years(string value, int startYear)
    {
        var parsed = SchoolYear.TryParse(value, out var schoolYear);

        Assert.True(parsed);
        Assert.NotNull(schoolYear);
        Assert.Equal(startYear, schoolYear.StartYear);
        Assert.Equal(startYear + 1, schoolYear.EndYear);
        Assert.Equal(value, schoolYear.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("2025")]
    [InlineData("2025-26")]
    [InlineData("2025/2026")]
    [InlineData(" 2025-2026")]
    [InlineData("2025-2026 ")]
    [InlineData("20a5-2026")]
    [InlineData("+025-2026")]
    [InlineData("2025-20266")]
    [InlineData("２０２５-２０２６")]
    [InlineData("0000-0001")]
    public void TryParse_rejects_malformed_values(string? value)
    {
        var parsed = SchoolYear.TryParse(value, out var schoolYear);

        Assert.False(parsed);
        Assert.Null(schoolYear);
    }

    [Theory]
    [InlineData("2025-2025")]
    [InlineData("2025-2027")]
    [InlineData("2026-2025")]
    public void TryParse_rejects_non_consecutive_years(string value)
    {
        Assert.False(SchoolYear.TryParse(value, out _));
    }

    [Fact]
    public void Parse_returns_the_school_year()
    {
        Assert.Equal(2025, SchoolYear.Parse("2025-2026").StartYear);
    }

    [Fact]
    public void Parse_throws_a_domain_exception_for_an_invalid_value()
    {
        var exception = Assert.Throws<DomainException>(() => SchoolYear.Parse("2025-2027"));

        Assert.Contains("2025-2027", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void School_years_with_the_same_name_are_equal()
    {
        Assert.Equal(SchoolYear.Parse("2025-2026"), SchoolYear.Parse("2025-2026"));
        Assert.NotEqual(SchoolYear.Parse("2025-2026"), SchoolYear.Parse("2026-2027"));
    }
}
