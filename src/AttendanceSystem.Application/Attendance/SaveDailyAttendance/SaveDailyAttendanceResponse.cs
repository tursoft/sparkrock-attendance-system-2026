namespace AttendanceSystem.Application.Attendance.SaveDailyAttendance;

/// <summary>The outcome of a daily save: each student's absence total for the school year (#43).</summary>
public sealed record SaveDailyAttendanceResponse(
    int SchoolId,
    DateOnly Date,
    string SchoolYear,
    int Threshold,
    IReadOnlyList<StudentAbsenceResult> Students);

public sealed record StudentAbsenceResult(int StudentId, int TotalAbsences, bool IsChronicallyAbsent, bool AlertRaised);
