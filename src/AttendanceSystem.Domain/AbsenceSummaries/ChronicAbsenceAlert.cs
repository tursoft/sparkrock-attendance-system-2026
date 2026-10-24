using System.Globalization;

namespace AttendanceSystem.Domain.AbsenceSummaries;

/// <summary>An alert raised when a student reaches the absence threshold. <see cref="Id"/> is 0 until persisted.</summary>
public sealed class ChronicAbsenceAlert
{
    public const string TypeName = "CHRONIC_ABSENCE";

    private ChronicAbsenceAlert(int id, DateTime alertDate, string? message)
    {
        Id = id;
        AlertDate = alertDate;
        Message = message;
    }

    public int Id { get; }

    public string AlertType { get; } = TypeName;

    public DateTime AlertDate { get; }

    public string? Message { get; }

    public static ChronicAbsenceAlert Restore(int id, DateTime alertDate, string? message) => new(id, alertDate, message);

    internal static ChronicAbsenceAlert Raise(int totalAbsences, DateTime now) =>
        new(0, now, string.Create(CultureInfo.InvariantCulture, $"Student has reached {totalAbsences} absences this year"));
}
