using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Application.Common;

/// <summary>Settings bound from the <c>Attendance</c> configuration section.</summary>
public sealed class AttendanceOptions
{
    public const string SectionName = "Attendance";

    /// <summary>Written to <c>CreatedBy</c>, <c>ModifiedBy</c> and <c>SubmittedBy</c> until authentication exists (#42).</summary>
    [Required]
    [MaxLength(100)]
    public string AuditUser { get; init; } = string.Empty;
}
