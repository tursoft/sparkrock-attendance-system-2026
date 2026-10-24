using AttendanceSystem.Domain.Attendance;
using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Tests.Attendance;

public class AttendanceRecordTests
{
    private const string User = "attendance-api";

    private static readonly AttendanceCode Present = new("P", "Present", isAbsent: false, isExcused: false, isActive: true);
    private static readonly AttendanceCode Absent = new("A", "Absent", isAbsent: true, isExcused: false, isActive: true);
    private static readonly AttendanceCode Excused = new("E", "Excused absence", isAbsent: true, isExcused: true, isActive: true);
    private static readonly AttendanceCode Retired = new("X", "Retired", isAbsent: true, isExcused: false, isActive: false);

    private static readonly SchoolYear Year = SchoolYear.Parse("2025-2026");
    private static readonly Term Fall = new(4, "Fall", Year, new DateOnly(2025, 9, 2), new DateOnly(2025, 12, 19));
    private static readonly Term OtherSchoolFall = new(9, "Autumn", Year, new DateOnly(2025, 9, 8), new DateOnly(2025, 12, 12));
    private static readonly Term Winter = new(5, "Winter", Year, new DateOnly(2026, 1, 5), new DateOnly(2026, 3, 27));

    private static readonly DateOnly Date = new(2025, 10, 6);
    private static readonly DateTime CreatedAt = new(2025, 10, 6, 14, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RevisedAt = new(2025, 10, 7, 9, 30, 0, DateTimeKind.Utc);

    private static AttendanceRecord RecordAbsence(
        AttendanceCode? code = null, int minutesLate = 0, string? notes = "Called in sick", Term? term = null) =>
        AttendanceRecord.Record(12, 3, Date, term ?? Fall, code ?? Absent, minutesLate, notes, CreatedAt, User);

    [Fact]
    public void Record_creates_an_unsaved_record_with_the_creation_audit()
    {
        var record = RecordAbsence();

        Assert.Equal(0, record.Id);
        Assert.Equal(12, record.StudentId);
        Assert.Equal(3, record.SchoolId);
        Assert.Equal(Date, record.Date);
        Assert.Equal(4, record.TermId);
        Assert.Equal("A", record.Code);
        Assert.True(record.IsAbsent);
        Assert.False(record.IsExcused);
        Assert.Equal(0, record.MinutesLate);
        Assert.Equal("Called in sick", record.Notes);
        Assert.Equal(CreatedAt, record.CreatedAt);
        Assert.Equal(User, record.CreatedBy);
        Assert.Null(record.ModifiedAt);
        Assert.Null(record.ModifiedBy);
    }

    [Fact]
    public void Record_in_a_term_break_has_no_term()
    {
        var record = AttendanceRecord.Record(12, 3, new DateOnly(2025, 12, 24), null, Absent, 0, null, CreatedAt, User);

        Assert.Null(record.TermId);
    }

    [Theory]
    [InlineData("P", false, false)]
    [InlineData("A", true, false)]
    [InlineData("E", true, true)]
    public void Record_copies_the_absent_and_excused_flags_from_the_code(string codeValue, bool isAbsent, bool isExcused)
    {
        var code = new[] { Present, Absent, Excused }.Single(code => code.Value == codeValue);

        var record = RecordAbsence(code);

        Assert.Equal(codeValue, record.Code);
        Assert.Equal(isAbsent, record.IsAbsent);
        Assert.Equal(isExcused, record.IsExcused);
    }

    [Fact]
    public void Records_of_different_students_do_not_share_code_flags()
    {
        var excused = AttendanceRecord.Record(12, 3, Date, Fall, Excused, 0, null, CreatedAt, User);
        var present = AttendanceRecord.Record(13, 3, Date, Fall, Present, 0, null, CreatedAt, User);

        Assert.Equal((12, true, true), (excused.StudentId, excused.IsAbsent, excused.IsExcused));
        Assert.Equal((13, false, false), (present.StudentId, present.IsAbsent, present.IsExcused));
    }

    [Fact]
    public void Record_rejects_an_inactive_code()
    {
        var exception = Assert.Throws<DomainException>(() => RecordAbsence(Retired));

        Assert.Contains("'X'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Record_rejects_a_term_that_does_not_cover_the_date()
    {
        Assert.Throws<DomainException>(() => RecordAbsence(term: Winter));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(600)]
    public void Record_accepts_minutes_late_from_0_to_600(int minutesLate)
    {
        Assert.Equal(minutesLate, RecordAbsence(minutesLate: minutesLate).MinutesLate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(601)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Record_rejects_minutes_late_outside_0_to_600(int minutesLate)
    {
        Assert.Throws<DomainException>(() => RecordAbsence(minutesLate: minutesLate));
    }

    [Theory]
    [InlineData("  Called in sick  ", "Called in sick")]
    [InlineData("\tDentist\r\n", "Dentist")]
    [InlineData("'; DROP TABLE Students; --", "'; DROP TABLE Students; --")]
    [InlineData("\"/><r sid=\"1\"", "\"/><r sid=\"1\"")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void Record_trims_notes_and_stores_empty_notes_as_null(string? notes, string? expected)
    {
        Assert.Equal(expected, RecordAbsence(notes: notes).Notes);
    }

    [Fact]
    public void Record_accepts_notes_of_500_characters_after_trimming()
    {
        var notes = new string('n', 500);

        Assert.Equal(notes, RecordAbsence(notes: $"  {notes}  ").Notes);
    }

    [Fact]
    public void Record_rejects_notes_longer_than_500_characters()
    {
        Assert.Throws<DomainException>(() => RecordAbsence(notes: new string('n', 501)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Record_requires_an_audit_user(string? user)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => AttendanceRecord.Record(12, 3, Date, Fall, Absent, 0, null, CreatedAt, user!));
    }

    [Fact]
    public void Revise_keeps_the_creation_audit_and_sets_the_modification_audit()
    {
        var record = RecordAbsence();

        record.Revise(3, Fall, Present, 0, null, RevisedAt, "clerk");

        Assert.Equal(CreatedAt, record.CreatedAt);
        Assert.Equal(User, record.CreatedBy);
        Assert.Equal(RevisedAt, record.ModifiedAt);
        Assert.Equal("clerk", record.ModifiedBy);
    }

    [Fact]
    public void Revise_replaces_the_code_snapshot_minutes_late_and_notes()
    {
        var record = RecordAbsence();

        record.Revise(3, Fall, Excused, 20, "  Doctor's note  ", RevisedAt, User);

        Assert.Equal("E", record.Code);
        Assert.True(record.IsAbsent);
        Assert.True(record.IsExcused);
        Assert.Equal(20, record.MinutesLate);
        Assert.Equal("Doctor's note", record.Notes);
    }

    [Fact]
    public void Revise_clears_notes_left_empty()
    {
        var record = RecordAbsence(notes: "Called in sick");

        record.Revise(3, Fall, Absent, 0, " ", RevisedAt, User);

        Assert.Null(record.Notes);
    }

    [Fact]
    public void Revise_keeps_the_identity_of_the_record()
    {
        var record = Restored();

        record.Revise(3, Fall, Present, 0, null, RevisedAt, User);

        Assert.Equal((41, 12, Date), (record.Id, record.StudentId, record.Date));
    }

    [Fact]
    public void Revise_corrects_the_school_and_term()
    {
        var record = RecordAbsence();

        record.Revise(8, OtherSchoolFall, Absent, 0, null, RevisedAt, User);

        Assert.Equal(8, record.SchoolId);
        Assert.Equal(9, record.TermId);
    }

    [Fact]
    public void Revise_clears_the_term_when_none_covers_the_date()
    {
        var record = RecordAbsence();

        record.Revise(3, null, Absent, 0, null, RevisedAt, User);

        Assert.Null(record.TermId);
    }

    [Fact]
    public void Revise_rejects_an_inactive_code_and_leaves_the_record_unchanged()
    {
        var record = RecordAbsence();

        Assert.Throws<DomainException>(() => record.Revise(8, OtherSchoolFall, Retired, 0, "changed", RevisedAt, "clerk"));

        AssertUnchanged(record);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(601)]
    public void Revise_rejects_minutes_late_outside_0_to_600_and_leaves_the_record_unchanged(int minutesLate)
    {
        var record = RecordAbsence();

        Assert.Throws<DomainException>(() => record.Revise(8, OtherSchoolFall, Present, minutesLate, "changed", RevisedAt, "clerk"));

        AssertUnchanged(record);
    }

    [Fact]
    public void Revise_rejects_notes_longer_than_500_characters_and_leaves_the_record_unchanged()
    {
        var record = RecordAbsence();

        Assert.Throws<DomainException>(() => record.Revise(8, OtherSchoolFall, Present, 0, new string('n', 501), RevisedAt, "clerk"));

        AssertUnchanged(record);
    }

    [Fact]
    public void Revise_rejects_a_term_that_does_not_cover_the_date()
    {
        var record = RecordAbsence();

        Assert.Throws<DomainException>(() => record.Revise(3, Winter, Present, 0, null, RevisedAt, User));

        AssertUnchanged(record);
    }

    [Fact]
    public void Revise_requires_an_audit_user()
    {
        var record = RecordAbsence();

        Assert.ThrowsAny<ArgumentException>(() => record.Revise(3, Fall, Present, 0, null, RevisedAt, " "));

        AssertUnchanged(record);
    }

    [Fact]
    public void Restore_sets_every_persisted_field()
    {
        var record = Restored();

        Assert.Equal(41, record.Id);
        Assert.Equal(12, record.StudentId);
        Assert.Equal(3, record.SchoolId);
        Assert.Equal(Date, record.Date);
        Assert.Equal(4, record.TermId);
        Assert.Equal("P", record.Code);
        Assert.True(record.IsAbsent);
        Assert.False(record.IsExcused);
        Assert.Equal(5, record.MinutesLate);
        Assert.Equal("Stored note", record.Notes);
        Assert.Equal(CreatedAt, record.CreatedAt);
        Assert.Null(record.CreatedBy);
        Assert.Equal(RevisedAt, record.ModifiedAt);
        Assert.Equal("clerk", record.ModifiedBy);
    }

    [Fact]
    public void Revising_a_restored_record_keeps_its_creation_audit()
    {
        var record = Restored();
        var later = RevisedAt.AddDays(1);

        record.Revise(3, Fall, Absent, 0, null, later, User);

        Assert.Equal(CreatedAt, record.CreatedAt);
        Assert.Null(record.CreatedBy);
        Assert.Equal(later, record.ModifiedAt);
        Assert.Equal(User, record.ModifiedBy);
    }

    private static AttendanceRecord Restored() =>
        AttendanceRecord.Restore(
            id: 41,
            studentId: 12,
            schoolId: 3,
            date: Date,
            termId: 4,
            code: "P",
            isAbsent: true,
            isExcused: false,
            minutesLate: 5,
            notes: "Stored note",
            createdAt: CreatedAt,
            createdBy: null,
            modifiedAt: RevisedAt,
            modifiedBy: "clerk");

    private static void AssertUnchanged(AttendanceRecord record)
    {
        Assert.Equal(3, record.SchoolId);
        Assert.Equal(4, record.TermId);
        Assert.Equal("A", record.Code);
        Assert.True(record.IsAbsent);
        Assert.False(record.IsExcused);
        Assert.Equal(0, record.MinutesLate);
        Assert.Equal("Called in sick", record.Notes);
        Assert.Null(record.ModifiedAt);
        Assert.Null(record.ModifiedBy);
    }
}
