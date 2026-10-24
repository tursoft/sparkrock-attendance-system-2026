#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class AttendanceSubmissionLog
{
    public int SubmissionId { get; set; }

    public int SchoolId { get; set; }

    public DateTime SubmittedDate { get; set; }

    public DateOnly AttendDate { get; set; }

    public int RecordCount { get; set; }

    public string? SubmittedBy { get; set; }

    public virtual School School { get; set; } = null!;
}
