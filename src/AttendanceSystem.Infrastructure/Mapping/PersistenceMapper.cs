using AttendanceSystem.Domain.AbsenceSummaries;
using AttendanceSystem.Domain.Schools;
using Riok.Mapperly.Abstractions;
using Domain = AttendanceSystem.Domain;
using Entities = AttendanceSystem.Infrastructure.Persistence.Entities;

namespace AttendanceSystem.Infrastructure.Mapping;

/// <summary>
/// Persistence entity ↔ domain mappings. Aggregates rehydrated through a <c>Restore</c> factory are mapped by hand,
/// because Mapperly maps to constructors only.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class PersistenceMapper
{
    [MapProperty(nameof(Entities.School.SchoolId), "id")]
    [MapProperty(nameof(Entities.School.Active), "isActive")]
    [MapProperty(nameof(Entities.School.TimeZone), "timeZoneId")]
    [MapProperty(nameof(Entities.School.SchoolTerms), "terms")]
    public static partial School ToDomain(this Entities.School school);

    [MapProperty(nameof(Entities.SchoolTerm.TermId), "id")]
    [MapProperty(nameof(Entities.SchoolTerm.TermName), "name")]
    public static partial Term ToDomain(this Entities.SchoolTerm term);

    [MapProperty(nameof(Entities.Student.StudentId), "id")]
    [MapProperty(nameof(Entities.Student.Active), "isActive")]
    public static partial Domain.Students.Student ToDomain(this Entities.Student student);

    [MapProperty(nameof(Entities.AttendanceCode.CodeValue), "value")]
    public static partial Domain.Attendance.AttendanceCode ToDomain(this Entities.AttendanceCode code);

    public static Domain.Attendance.AttendanceRecord ToDomain(this Entities.StudentAttendance entity) =>
        Domain.Attendance.AttendanceRecord.Restore(
            entity.AttendanceId,
            entity.StudentId,
            entity.SchoolId,
            entity.AttendDate,
            entity.TermId,
            entity.AttendCode,
            entity.IsAbsent,
            entity.IsExcused,
            entity.MinutesLate ?? 0,
            entity.Notes,
            entity.CreatedDate,
            entity.CreatedBy,
            entity.ModifiedDate,
            entity.ModifiedBy);

    public static AbsenceSummary ToDomain(this Entities.StudentAttendanceSummary entity, Entities.StudentAlert? openAlert) =>
        AbsenceSummary.Restore(
            entity.SummaryId,
            entity.StudentId,
            entity.SchoolId,
            SchoolYear.Parse(entity.SchoolYear),
            entity.TotalAbsences,
            entity.LastUpdated,
            openAlert is null ? null : ChronicAbsenceAlert.Restore(openAlert.AlertId, openAlert.AlertDate, openAlert.AlertMessage));

    /// <summary>Copies the record's state onto a new or tracked entity; the key and row version stay as they are.</summary>
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.AttendanceId))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.RowVersion))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.AttendCodeNavigation))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.School))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.Student))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendance.Term))]
    [MapProperty(nameof(Domain.Attendance.AttendanceRecord.Date), nameof(Entities.StudentAttendance.AttendDate))]
    [MapProperty(nameof(Domain.Attendance.AttendanceRecord.Code), nameof(Entities.StudentAttendance.AttendCode))]
    [MapProperty(nameof(Domain.Attendance.AttendanceRecord.CreatedAt), nameof(Entities.StudentAttendance.CreatedDate))]
    [MapProperty(nameof(Domain.Attendance.AttendanceRecord.ModifiedAt), nameof(Entities.StudentAttendance.ModifiedDate))]
    public static partial void CopyTo(this Domain.Attendance.AttendanceRecord record, Entities.StudentAttendance entity);

    /// <summary>Copies the summary's totals onto a new or tracked entity; the key and row version stay as they are.</summary>
    [MapperIgnoreTarget(nameof(Entities.StudentAttendanceSummary.SummaryId))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendanceSummary.RowVersion))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendanceSummary.School))]
    [MapperIgnoreTarget(nameof(Entities.StudentAttendanceSummary.Student))]
    public static partial void CopyTo(this AbsenceSummary summary, Entities.StudentAttendanceSummary entity);

    [MapProperty(nameof(Domain.Attendance.AttendanceSubmission.SubmittedAt), nameof(Entities.AttendanceSubmissionLog.SubmittedDate))]
    [MapperIgnoreTarget(nameof(Entities.AttendanceSubmissionLog.SubmissionId))]
    [MapperIgnoreTarget(nameof(Entities.AttendanceSubmissionLog.School))]
    public static partial Entities.AttendanceSubmissionLog ToEntity(this Domain.Attendance.AttendanceSubmission submission);

    public static Entities.StudentAlert ToEntity(this ChronicAbsenceAlert alert, AbsenceSummary summary) => new()
    {
        StudentId = summary.StudentId,
        SchoolId = summary.SchoolId,
        SchoolYear = summary.SchoolYear.ToString(),
        AlertType = alert.AlertType,
        AlertDate = alert.AlertDate,
        AlertMessage = alert.Message,
    };

    private static SchoolYear ToSchoolYear(string schoolYear) => SchoolYear.Parse(schoolYear);

    private static string FromSchoolYear(SchoolYear schoolYear) => schoolYear.ToString();
}
