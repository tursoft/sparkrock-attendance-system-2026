namespace AttendanceSystem.Application.Students.AttendanceHistory;

public sealed record AttendanceHistoryItemDto(
    DateOnly Date,
    int SchoolId,
    string Code,
    string CodeDescription,
    bool IsAbsent,
    bool IsExcused,
    int MinutesLate,
    string? Notes,
    string? TermName);

public sealed record AttendanceHistoryDto(
    string SchoolYear,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<AttendanceHistoryItemDto> Items);
