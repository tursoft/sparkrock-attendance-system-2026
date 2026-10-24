#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class StudentAttendanceSummary
{
    public int SummaryId { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public string SchoolYear { get; set; } = null!;

    public int TotalAbsences { get; set; }

    public DateTime LastUpdated { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
