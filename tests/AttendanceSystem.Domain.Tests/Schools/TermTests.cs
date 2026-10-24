using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Tests.Schools;

public class TermTests
{
    private static readonly SchoolYear Year = SchoolYear.Parse("2025-2026");

    [Fact]
    public void Keeps_its_details()
    {
        var term = new Term(7, "Fall", Year, new DateOnly(2025, 9, 2), new DateOnly(2025, 12, 19));

        Assert.Equal(7, term.Id);
        Assert.Equal("Fall", term.Name);
        Assert.Equal(Year, term.SchoolYear);
        Assert.Equal(new DateOnly(2025, 9, 2), term.StartDate);
        Assert.Equal(new DateOnly(2025, 12, 19), term.EndDate);
    }

    [Theory]
    [InlineData(2025, 9, 2, true)]
    [InlineData(2025, 10, 15, true)]
    [InlineData(2025, 12, 19, true)]
    [InlineData(2025, 9, 1, false)]
    [InlineData(2025, 12, 20, false)]
    public void Contains_dates_from_start_to_end_inclusive(int year, int month, int day, bool expected)
    {
        var term = new Term(1, "Fall", Year, new DateOnly(2025, 9, 2), new DateOnly(2025, 12, 19));

        Assert.Equal(expected, term.Contains(new DateOnly(year, month, day)));
    }

    [Fact]
    public void A_one_day_term_is_valid()
    {
        var day = new DateOnly(2025, 9, 2);

        Assert.True(new Term(1, "Orientation", Year, day, day).Contains(day));
    }

    [Fact]
    public void Rejects_an_end_date_before_the_start_date()
    {
        Assert.Throws<DomainException>(
            () => new Term(1, "Fall", Year, new DateOnly(2025, 12, 19), new DateOnly(2025, 9, 2)));
    }
}
