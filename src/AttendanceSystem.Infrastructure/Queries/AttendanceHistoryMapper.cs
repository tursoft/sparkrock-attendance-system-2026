using AttendanceSystem.Application.Students.AttendanceHistory;
using AttendanceSystem.Infrastructure.Persistence.Entities;
using Riok.Mapperly.Abstractions;

namespace AttendanceSystem.Infrastructure.Queries;

/// <summary>Projects history rows straight to the DTO, so EF selects only its columns.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class AttendanceHistoryMapper
{
    public static partial IQueryable<AttendanceHistoryItemDto> ProjectToHistoryItem(this IQueryable<StudentAttendance> records);

    [MapProperty(nameof(StudentAttendance.AttendDate), nameof(AttendanceHistoryItemDto.Date))]
    [MapProperty(nameof(StudentAttendance.AttendCode), nameof(AttendanceHistoryItemDto.Code))]
    [MapProperty(
        [nameof(StudentAttendance.AttendCodeNavigation), nameof(AttendanceCode.Description)],
        nameof(AttendanceHistoryItemDto.CodeDescription))]
    [MapProperty([nameof(StudentAttendance.Term), nameof(SchoolTerm.TermName)], nameof(AttendanceHistoryItemDto.TermName))]
    private static partial AttendanceHistoryItemDto ToHistoryItem(StudentAttendance record);
}
