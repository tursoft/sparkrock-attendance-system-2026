using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.AbsenceSummaries;

/// <summary>A student's absence total at one school in one school year. <see cref="Id"/> is 0 until persisted.</summary>
public sealed class AbsenceSummary
{
    private AbsenceSummary(
        int id,
        int studentId,
        int schoolId,
        SchoolYear schoolYear,
        int totalAbsences,
        DateTime lastUpdated,
        ChronicAbsenceAlert? openAlert)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);

        Id = id;
        StudentId = studentId;
        SchoolId = schoolId;
        SchoolYear = schoolYear;
        TotalAbsences = totalAbsences;
        LastUpdated = lastUpdated;
        OpenAlert = openAlert;
    }

    public int Id { get; }

    public int StudentId { get; }

    public int SchoolId { get; }

    public SchoolYear SchoolYear { get; }

    public int TotalAbsences { get; private set; }

    public DateTime LastUpdated { get; private set; }

    /// <summary>The unresolved chronic-absence alert, if any. Alerts are never resolved automatically.</summary>
    public ChronicAbsenceAlert? OpenAlert { get; private set; }

    /// <summary>The alert raised by <see cref="Recalculate"/> on this instance, which still has to be saved.</summary>
    public ChronicAbsenceAlert? RaisedAlert { get; private set; }

    public static AbsenceSummary Start(int studentId, int schoolId, SchoolYear schoolYear, DateTime now) =>
        new(0, studentId, schoolId, schoolYear, 0, now, null);

    /// <summary>Rehydrates a persisted summary as stored, with its open alert if one exists.</summary>
    public static AbsenceSummary Restore(
        int id,
        int studentId,
        int schoolId,
        SchoolYear schoolYear,
        int totalAbsences,
        DateTime lastUpdated,
        ChronicAbsenceAlert? openAlert) =>
        new(id, studentId, schoolId, schoolYear, totalAbsences, lastUpdated, openAlert);

    public bool IsChronicallyAbsent(int threshold) => TotalAbsences >= threshold;

    /// <summary>Sets the new total and raises an alert when it reaches the threshold and none is open.</summary>
    /// <returns>Whether a new alert was raised.</returns>
    public bool Recalculate(int totalAbsences, int threshold, DateTime now)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalAbsences);

        TotalAbsences = totalAbsences;
        LastUpdated = now;
        if (!IsChronicallyAbsent(threshold) || OpenAlert is not null)
        {
            return false;
        }

        RaisedAlert = ChronicAbsenceAlert.Raise(totalAbsences, now);
        OpenAlert = RaisedAlert;
        return true;
    }
}
