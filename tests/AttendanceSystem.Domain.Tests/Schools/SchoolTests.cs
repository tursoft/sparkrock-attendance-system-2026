using System.Globalization;
using AttendanceSystem.Domain.Schools;
using Microsoft.Extensions.Time.Testing;

namespace AttendanceSystem.Domain.Tests.Schools;

public class SchoolTests
{
    private static readonly SchoolYear Year2025 = SchoolYear.Parse("2025-2026");
    private static readonly SchoolYear Year2026 = SchoolYear.Parse("2026-2027");

    private static readonly Term Fall2025 = new(1, "Fall", Year2025, new DateOnly(2025, 9, 2), new DateOnly(2025, 12, 19));
    private static readonly Term Winter2026 = new(2, "Winter", Year2025, new DateOnly(2026, 1, 5), new DateOnly(2026, 3, 27));
    private static readonly Term Spring2026 = new(3, "Spring", Year2025, new DateOnly(2026, 4, 6), new DateOnly(2026, 6, 26));
    private static readonly Term Fall2026 = new(4, "Fall", Year2026, new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 18));

    private static School SchoolWith(params Term[] terms) => new(3, true, "UTC", null, terms);

    private static School TwoYearSchool() => SchoolWith(Spring2026, Fall2026, Fall2025, Winter2026);

    private static DateOnly Day(string isoDate) => DateOnly.Parse(isoDate, CultureInfo.InvariantCulture);

    [Fact]
    public void Keeps_its_details()
    {
        var school = new School(3, false, "America/Los_Angeles", 7, [Fall2025]);

        Assert.Equal(3, school.Id);
        Assert.False(school.IsActive);
        Assert.Equal("America/Los_Angeles", school.TimeZoneId);
        Assert.Equal(7, school.AbsenceAlertThreshold);
        Assert.Equal([Fall2025], school.Terms);
    }

    [Fact]
    public void Absence_alert_threshold_defaults_to_ten_when_not_set()
    {
        var school = new School(3, true, "UTC", null, []);

        Assert.Equal(10, School.DefaultAbsenceAlertThreshold);
        Assert.Equal(School.DefaultAbsenceAlertThreshold, school.AbsenceAlertThreshold);
    }

    [Theory]
    [InlineData("Mars/Olympus_Mons")]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_an_unknown_time_zone(string timeZoneId)
    {
        Assert.Throws<DomainException>(() => new School(3, true, timeZoneId, null, []));
    }

    [Theory]
    [InlineData("2026-01-15T23:30:00Z", "Pacific/Kiritimati", "2026-01-16")]
    [InlineData("2026-01-15T23:30:00Z", "UTC", "2026-01-15")]
    [InlineData("2026-01-15T23:30:00Z", "America/Los_Angeles", "2026-01-15")]
    [InlineData("2026-01-16T00:30:00Z", "Pacific/Kiritimati", "2026-01-16")]
    [InlineData("2026-01-16T00:30:00Z", "UTC", "2026-01-16")]
    [InlineData("2026-01-16T00:30:00Z", "America/Los_Angeles", "2026-01-15")]
    [InlineData("2026-01-15T09:59:59Z", "Pacific/Kiritimati", "2026-01-15")]
    [InlineData("2026-01-15T10:00:00Z", "Pacific/Kiritimati", "2026-01-16")]
    [InlineData("2026-07-01T06:59:59Z", "America/Los_Angeles", "2026-06-30")]
    [InlineData("2026-07-01T07:00:00Z", "America/Los_Angeles", "2026-07-01")]
    public void Today_is_the_local_date_in_the_school_time_zone(string utcNow, string timeZoneId, string expectedToday)
    {
        var clock = new FakeTimeProvider(DateTimeOffset.Parse(utcNow, CultureInfo.InvariantCulture));
        var school = new School(3, true, timeZoneId, null, []);

        Assert.Equal(Day(expectedToday), school.Today(clock));
    }

    [Theory]
    [InlineData("2025-09-02")]
    [InlineData("2025-12-25")]
    [InlineData("2026-04-01")]
    [InlineData("2026-06-26")]
    public void School_year_spans_from_the_earliest_term_start_to_the_latest_term_end(string date)
    {
        var year = TwoYearSchool().SchoolYearOn(Day(date));

        Assert.NotNull(year);
        Assert.Equal(Year2025, year.SchoolYear);
        Assert.Equal(Fall2025.StartDate, year.StartDate);
        Assert.Equal(Spring2026.EndDate, year.EndDate);
    }

    [Theory]
    [InlineData("2025-09-01")]
    [InlineData("2026-07-15")]
    [InlineData("2026-12-19")]
    public void No_school_year_covers_a_date_outside_every_year(string date)
    {
        Assert.Null(TwoYearSchool().SchoolYearOn(Day(date)));
    }

    [Fact]
    public void A_school_without_terms_has_no_school_year()
    {
        var school = SchoolWith();

        Assert.Null(school.SchoolYearOn(Day("2026-01-15")));
        Assert.Null(school.CurrentSchoolYear(Day("2026-01-15")));
        Assert.Null(school.FindSchoolYear(Year2025));
    }

    [Theory]
    [InlineData("2025-09-02", "2025-2026")]
    [InlineData("2026-03-01", "2025-2026")]
    [InlineData("2026-07-15", "2025-2026")]
    [InlineData("2026-08-31", "2025-2026")]
    [InlineData("2026-09-01", "2026-2027")]
    [InlineData("2027-08-01", "2026-2027")]
    public void Current_school_year_is_the_latest_year_started_on_or_before_today(string today, string expected)
    {
        var year = TwoYearSchool().CurrentSchoolYear(Day(today));

        Assert.NotNull(year);
        Assert.Equal(SchoolYear.Parse(expected), year.SchoolYear);
    }

    [Fact]
    public void No_current_school_year_before_the_first_year_starts()
    {
        Assert.Null(TwoYearSchool().CurrentSchoolYear(Day("2025-09-01")));
    }

    [Fact]
    public void Finds_a_school_year_by_name()
    {
        var year = TwoYearSchool().FindSchoolYear(Year2026);

        Assert.NotNull(year);
        Assert.Equal(Year2026, year.SchoolYear);
        Assert.Equal(Fall2026.StartDate, year.StartDate);
        Assert.Equal(Fall2026.EndDate, year.EndDate);
    }

    [Fact]
    public void Does_not_find_a_school_year_without_terms()
    {
        Assert.Null(TwoYearSchool().FindSchoolYear(SchoolYear.Parse("2030-2031")));
    }

    [Theory]
    [InlineData("2025-09-02", 1)]
    [InlineData("2025-12-19", 1)]
    [InlineData("2026-02-10", 2)]
    [InlineData("2026-06-26", 3)]
    [InlineData("2026-10-01", 4)]
    public void Term_on_a_date_is_the_term_containing_it(string date, int expectedTermId)
    {
        Assert.Equal(expectedTermId, TwoYearSchool().TermOn(Day(date))?.Id);
    }

    [Theory]
    [InlineData("2025-12-25")]
    [InlineData("2026-07-15")]
    [InlineData("2025-08-01")]
    public void No_term_on_a_date_in_a_break(string date)
    {
        Assert.Null(TwoYearSchool().TermOn(Day(date)));
    }

    [Fact]
    public void Overlapping_terms_resolve_to_the_earliest_start()
    {
        var later = new Term(3, "Later", Year2025, new DateOnly(2025, 11, 1), new DateOnly(2026, 1, 31));
        var earlier = new Term(5, "Earlier", Year2025, new DateOnly(2025, 9, 1), new DateOnly(2025, 12, 31));

        Assert.Equal(5, SchoolWith(later, earlier).TermOn(Day("2025-11-15"))?.Id);
    }

    [Fact]
    public void Overlapping_terms_with_the_same_start_resolve_to_the_lowest_id()
    {
        var higherId = new Term(9, "Higher", Year2025, new DateOnly(2025, 9, 1), new DateOnly(2025, 12, 31));
        var lowerId = new Term(4, "Lower", Year2025, new DateOnly(2025, 9, 1), new DateOnly(2025, 10, 31));

        Assert.Equal(4, SchoolWith(higherId, lowerId).TermOn(Day("2025-10-15"))?.Id);
    }
}
