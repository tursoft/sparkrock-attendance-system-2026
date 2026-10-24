using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Tests.Schools;

public class SchoolYearPeriodTests
{
    private static readonly SchoolYear Year = SchoolYear.Parse("2025-2026");

    [Theory]
    [InlineData(2025, 9, 2, true)]
    [InlineData(2026, 6, 26, true)]
    [InlineData(2025, 9, 1, false)]
    [InlineData(2026, 6, 27, false)]
    public void Contains_dates_from_start_to_end_inclusive(int year, int month, int day, bool expected)
    {
        var period = new SchoolYearPeriod(Year, new DateOnly(2025, 9, 2), new DateOnly(2026, 6, 26));

        Assert.Equal(expected, period.Contains(new DateOnly(year, month, day)));
    }

    [Fact]
    public void Rejects_an_end_date_before_the_start_date()
    {
        Assert.Throws<DomainException>(
            () => new SchoolYearPeriod(Year, new DateOnly(2026, 6, 26), new DateOnly(2025, 9, 2)));
    }
}
