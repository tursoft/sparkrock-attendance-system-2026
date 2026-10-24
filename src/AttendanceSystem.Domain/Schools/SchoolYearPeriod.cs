namespace AttendanceSystem.Domain.Schools;

/// <summary>The dates a school year covers at one school: its earliest term start to its latest term end.</summary>
public sealed record SchoolYearPeriod
{
    public SchoolYearPeriod(SchoolYear schoolYear, DateOnly startDate, DateOnly endDate)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);
        if (endDate < startDate)
        {
            throw new DomainException($"School year {schoolYear} ends on {endDate:O}, before it starts on {startDate:O}.");
        }

        SchoolYear = schoolYear;
        StartDate = startDate;
        EndDate = endDate;
    }

    public SchoolYear SchoolYear { get; }

    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;
}
