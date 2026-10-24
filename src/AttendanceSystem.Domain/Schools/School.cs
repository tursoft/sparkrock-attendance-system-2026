namespace AttendanceSystem.Domain.Schools;

public sealed class School
{
    public const int DefaultAbsenceAlertThreshold = 10;

    private readonly TimeZoneInfo _timeZone;
    private readonly IReadOnlyList<SchoolYearPeriod> _schoolYears;

    public School(int id, bool isActive, string timeZoneId, int? absenceAlertThreshold, IEnumerable<Term> terms)
    {
        ArgumentNullException.ThrowIfNull(terms);

        Id = id;
        IsActive = isActive;
        TimeZoneId = timeZoneId;
        _timeZone = FindTimeZone(timeZoneId);
        AbsenceAlertThreshold = absenceAlertThreshold ?? DefaultAbsenceAlertThreshold;
        Terms = [.. terms.OrderBy(term => term.StartDate).ThenBy(term => term.Id)];
        _schoolYears =
        [
            .. Terms
                .GroupBy(term => term.SchoolYear)
                .Select(year => new SchoolYearPeriod(
                    year.Key, year.Min(term => term.StartDate), year.Max(term => term.EndDate)))
                .OrderBy(year => year.StartDate)
                .ThenBy(year => year.SchoolYear.StartYear),
        ];
    }

    public int Id { get; }

    public bool IsActive { get; }

    /// <summary>The IANA time zone id that decides the school's local date.</summary>
    public string TimeZoneId { get; }

    /// <summary>The effective threshold: the school's own value, or <see cref="DefaultAbsenceAlertThreshold"/> when it has none.</summary>
    public int AbsenceAlertThreshold { get; }

    /// <summary>Ordered by start date, then id.</summary>
    public IReadOnlyList<Term> Terms { get; }

    public DateOnly Today(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), _timeZone).DateTime);
    }

    public SchoolYearPeriod? SchoolYearOn(DateOnly date) => _schoolYears.FirstOrDefault(year => year.Contains(date));

    public SchoolYearPeriod? CurrentSchoolYear(DateOnly today) => _schoolYears.LastOrDefault(year => year.StartDate <= today);

    public SchoolYearPeriod? FindSchoolYear(SchoolYear schoolYear) => _schoolYears.FirstOrDefault(year => year.SchoolYear == schoolYear);

    /// <summary>The term containing the date. Overlapping terms resolve to the earliest start, then the lowest id.</summary>
    public Term? TermOn(DateOnly date) => Terms.FirstOrDefault(term => term.Contains(date));

    private static TimeZoneInfo FindTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new DomainException("A school needs a time zone.");
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new DomainException($"'{timeZoneId}' is not a known time zone.");
        }
    }
}
