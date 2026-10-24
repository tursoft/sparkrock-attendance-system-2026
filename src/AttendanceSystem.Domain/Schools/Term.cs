namespace AttendanceSystem.Domain.Schools;

public sealed class Term
{
    public Term(int id, string name, SchoolYear schoolYear, DateOnly startDate, DateOnly endDate)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(schoolYear);
        if (endDate < startDate)
        {
            throw new DomainException($"Term {id} ends on {endDate:O}, before it starts on {startDate:O}.");
        }

        Id = id;
        Name = name;
        SchoolYear = schoolYear;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int Id { get; }

    public string Name { get; }

    public SchoolYear SchoolYear { get; }

    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;
}
