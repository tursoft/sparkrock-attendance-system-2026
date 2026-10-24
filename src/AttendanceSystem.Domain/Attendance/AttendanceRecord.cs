using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Attendance;

/// <summary>A student's attendance on one date. <see cref="Id"/> is 0 until the record is persisted.</summary>
public sealed class AttendanceRecord
{
    public const int MaxMinutesLate = 600;
    public const int MaxNotesLength = 500;

    private AttendanceRecord(int id, int studentId, DateOnly date, DateTime createdAt, string? createdBy)
    {
        Id = id;
        StudentId = studentId;
        Date = date;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public int Id { get; }

    public int StudentId { get; }

    public int SchoolId { get; private set; }

    public DateOnly Date { get; }

    public int? TermId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    /// <summary>Copied from the code when saved, so later code changes don't rewrite history.</summary>
    public bool IsAbsent { get; private set; }

    /// <summary>Copied from the code when saved, so later code changes don't rewrite history.</summary>
    public bool IsExcused { get; private set; }

    public int MinutesLate { get; private set; }

    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; }

    public string? CreatedBy { get; }

    public DateTime? ModifiedAt { get; private set; }

    public string? ModifiedBy { get; private set; }

    public static AttendanceRecord Record(
        int studentId,
        int schoolId,
        DateOnly date,
        Term? term,
        AttendanceCode code,
        int minutesLate,
        string? notes,
        DateTime now,
        string user)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        var record = new AttendanceRecord(0, studentId, date, now, user);
        record.Apply(schoolId, term, code, minutesLate, notes);
        return record;
    }

    /// <summary>Rehydrates a persisted record as stored, without validation or audit changes.</summary>
    public static AttendanceRecord Restore(
        int id,
        int studentId,
        int schoolId,
        DateOnly date,
        int? termId,
        string code,
        bool isAbsent,
        bool isExcused,
        int minutesLate,
        string? notes,
        DateTime createdAt,
        string? createdBy,
        DateTime? modifiedAt,
        string? modifiedBy) =>
        new(id, studentId, date, createdAt, createdBy)
        {
            SchoolId = schoolId,
            TermId = termId,
            Code = code,
            IsAbsent = isAbsent,
            IsExcused = isExcused,
            MinutesLate = minutesLate,
            Notes = notes,
            ModifiedAt = modifiedAt,
            ModifiedBy = modifiedBy,
        };

    /// <summary>Overwrites a saved record. The school and term are corrected too, not only the code.</summary>
    public void Revise(int schoolId, Term? term, AttendanceCode code, int minutesLate, string? notes, DateTime now, string user)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        Apply(schoolId, term, code, minutesLate, notes);
        ModifiedAt = now;
        ModifiedBy = user;
    }

    private void Apply(int schoolId, Term? term, AttendanceCode code, int minutesLate, string? notes)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (!code.IsActive)
        {
            throw new DomainException($"Attendance code '{code.Value}' is inactive.");
        }

        if (term is not null && !term.Contains(Date))
        {
            throw new DomainException($"Term {term.Id} does not cover {Date:O}.");
        }

        if (minutesLate is < 0 or > MaxMinutesLate)
        {
            throw new DomainException($"Minutes late must be between 0 and {MaxMinutesLate}.");
        }

        var trimmedNotes = TrimNotes(notes);

        SchoolId = schoolId;
        TermId = term?.Id;
        Code = code.Value;
        IsAbsent = code.IsAbsent;
        IsExcused = code.IsExcused;
        MinutesLate = minutesLate;
        Notes = trimmedNotes;
    }

    private static string? TrimNotes(string? notes)
    {
        var trimmed = notes?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length <= MaxNotesLength
            ? trimmed
            : throw new DomainException($"Notes must be at most {MaxNotesLength} characters.");
    }
}
