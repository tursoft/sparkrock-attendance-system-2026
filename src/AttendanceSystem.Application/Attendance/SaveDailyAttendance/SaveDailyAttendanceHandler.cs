using AttendanceSystem.Application.Common;
using AttendanceSystem.Domain.AbsenceSummaries;
using AttendanceSystem.Domain.Attendance;
using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Domain.Students;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Application.Attendance.SaveDailyAttendance;

/// <summary>UC1: records or revises a school's attendance for one date and recalculates each student's absences.</summary>
public sealed partial class SaveDailyAttendanceHandler(
    ISchoolRepository schools,
    IStudentRepository students,
    IAttendanceCodeRepository codes,
    IAttendanceRecordRepository records,
    IAbsenceSummaryRepository summaries,
    IAttendanceSubmissionRepository submissions,
    IUnitOfWork unitOfWork,
    TimeProvider clock,
    IOptions<AttendanceOptions> options,
    ILogger<SaveDailyAttendanceHandler> logger)
{
    public async Task<Result<SaveDailyAttendanceResponse>> HandleAsync(
        int schoolId, DateOnly date, SaveDailyAttendanceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var school = await schools.GetAsync(schoolId, cancellationToken);
        if (school is null)
        {
            return Error.NotFound("school.not_found", $"School {schoolId} was not found.");
        }

        var schoolCheck = CheckSchoolAndDate(school, date);
        if (schoolCheck.IsFailure)
        {
            return schoolCheck.Error;
        }

        var schoolYear = schoolCheck.Value;
        var entries = request.Entries;
        var duplicateErrors = FindDuplicateStudents(entries);
        if (duplicateErrors.Count > 0)
        {
            return Error.Validation(duplicateErrors);
        }

        var studentIds = entries.Select(entry => entry.StudentId).ToList();
        var codeValues = entries.Select(entry => entry.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var studentsById = (await students.GetManyAsync(studentIds, cancellationToken)).ToDictionary(student => student.Id);
        var codesByValue = (await codes.GetManyAsync(codeValues, cancellationToken))
            .ToDictionary(code => code.Value, StringComparer.OrdinalIgnoreCase);

        var entryErrors = FindInvalidEntries(entries, schoolId, studentsById, codesByValue);
        if (entryErrors.Count > 0)
        {
            return Error.Validation(entryErrors);
        }

        var existingRecords = (await records.GetForDateAsync(studentIds, date, cancellationToken)).ToDictionary(record => record.StudentId);
        var existingSummaries = (await summaries.GetManyAsync(schoolId, schoolYear.SchoolYear, studentIds, cancellationToken))
            .ToDictionary(summary => summary.StudentId);
        var otherAbsences = await records.CountAbsencesAsync(
            schoolId, studentIds, schoolYear.StartDate, schoolYear.EndDate, date, cancellationToken);

        var now = clock.GetUtcNow().UtcDateTime;
        var user = options.Value.AuditUser;
        var term = school.TermOn(date);
        var results = new List<StudentAbsenceResult>(entries.Count);
        foreach (var entry in entries)
        {
            var code = codesByValue[entry.Code];
            var minutesLate = entry.MinutesLate ?? 0;
            if (existingRecords.TryGetValue(entry.StudentId, out var record))
            {
                record.Revise(schoolId, term, code, minutesLate, entry.Notes, now, user);
                records.Update(record);
            }
            else
            {
                record = AttendanceRecord.Record(entry.StudentId, schoolId, date, term, code, minutesLate, entry.Notes, now, user);
                records.Add(record);
            }

            var totalAbsences = otherAbsences.GetValueOrDefault(entry.StudentId) + (record.IsAbsent ? 1 : 0);
            var alertRaised = RecalculateSummary(existingSummaries, entry.StudentId, school, schoolYear.SchoolYear, totalAbsences, now);
            results.Add(new StudentAbsenceResult(
                entry.StudentId, totalAbsences, totalAbsences >= school.AbsenceAlertThreshold, alertRaised));
        }

        submissions.Add(new AttendanceSubmission(schoolId, date, entries.Count, now, user));

        var saved = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            LogSaveConflict(logger, schoolId, date);
            return saved.Error;
        }

        var alertCount = results.Count(result => result.AlertRaised);
        LogAttendanceSaved(logger, schoolId, date, entries.Count, alertCount);
        return new SaveDailyAttendanceResponse(
            schoolId, date, schoolYear.SchoolYear.ToString(), school.AbsenceAlertThreshold, results);
    }

    private Result<SchoolYearPeriod> CheckSchoolAndDate(School school, DateOnly date)
    {
        if (!school.IsActive)
        {
            return Error.Validation("schoolId", $"School {school.Id} is inactive.");
        }

        if (date > school.Today(clock))
        {
            return Error.Validation("date", "Attendance cannot be saved for a date after today in the school's time zone.");
        }

        var schoolYear = school.SchoolYearOn(date);
        return schoolYear is null
            ? Error.Validation("date", $"No school year of school {school.Id} covers {date:yyyy-MM-dd}.")
            : schoolYear;
    }

    private static Dictionary<string, string[]> FindDuplicateStudents(IReadOnlyList<AttendanceEntryRequest> entries)
    {
        var errors = new Dictionary<string, string[]>();
        var seen = new HashSet<int>();
        for (var i = 0; i < entries.Count; i++)
        {
            if (!seen.Add(entries[i].StudentId))
            {
                errors[$"entries[{i}].studentId"] = [$"Student {entries[i].StudentId} appears more than once."];
            }
        }

        return errors;
    }

    private static Dictionary<string, string[]> FindInvalidEntries(
        IReadOnlyList<AttendanceEntryRequest> entries,
        int schoolId,
        Dictionary<int, Student> studentsById,
        Dictionary<string, AttendanceCode> codesByValue)
    {
        var errors = new Dictionary<string, string[]>();
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!studentsById.TryGetValue(entry.StudentId, out var student) || !student.IsEnrolledAt(schoolId))
            {
                errors[$"entries[{i}].studentId"] = [$"Student {entry.StudentId} is not an active student of school {schoolId}."];
            }

            if (!codesByValue.TryGetValue(entry.Code, out var code) || !code.IsActive)
            {
                errors[$"entries[{i}].code"] = [$"'{entry.Code}' is not an active attendance code."];
            }
        }

        return errors;
    }

    private bool RecalculateSummary(
        Dictionary<int, AbsenceSummary> existingSummaries,
        int studentId,
        School school,
        SchoolYear schoolYear,
        int totalAbsences,
        DateTime now)
    {
        if (existingSummaries.TryGetValue(studentId, out var summary))
        {
            var raised = summary.Recalculate(totalAbsences, school.AbsenceAlertThreshold, now);
            summaries.Update(summary);
            return raised;
        }

        summary = AbsenceSummary.Start(studentId, school.Id, schoolYear, now);
        var alertRaised = summary.Recalculate(totalAbsences, school.AbsenceAlertThreshold, now);
        summaries.Add(summary);
        return alertRaised;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Attendance saved for {SchoolId} on {AttendDate}: {RecordCount} records, {AlertCount} alerts raised")]
    private static partial void LogAttendanceSaved(ILogger logger, int schoolId, DateOnly attendDate, int recordCount, int alertCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attendance save for {SchoolId} on {AttendDate} lost a concurrent race")]
    private static partial void LogSaveConflict(ILogger logger, int schoolId, DateOnly attendDate);
}
