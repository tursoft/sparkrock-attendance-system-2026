using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Domain.Attendance;

namespace AttendanceSystem.Application.Attendance.SaveDailyAttendance;

/// <summary>The day's attendance for some of a school's students. There is no cap on the number of entries (#48).</summary>
public sealed record SaveDailyAttendanceRequest(
    [property: Required, MinLength(1)] IReadOnlyList<AttendanceEntryRequest> Entries);

public sealed record AttendanceEntryRequest(
    [property: Range(1, int.MaxValue)] int StudentId,
    [property: Required, StringLength(5, MinimumLength = 1)] string Code,
    [property: Range(0, AttendanceRecord.MaxMinutesLate)] int? MinutesLate,
    [property: StringLength(AttendanceRecord.MaxNotesLength)] string? Notes);
