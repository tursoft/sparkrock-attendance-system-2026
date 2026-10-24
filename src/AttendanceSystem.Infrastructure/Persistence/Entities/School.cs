#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class School
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; } = null!;

    public bool Active { get; set; }

    public int? AbsenceAlertThreshold { get; set; }

    public string TimeZone { get; set; } = null!;

    public virtual ICollection<AttendanceSubmissionLog> AttendanceSubmissionLogs { get; set; } = new List<AttendanceSubmissionLog>();

    public virtual ICollection<SchoolTerm> SchoolTerms { get; set; } = new List<SchoolTerm>();

    public virtual ICollection<StudentAlert> StudentAlerts { get; set; } = new List<StudentAlert>();

    public virtual ICollection<StudentAttendanceSummary> StudentAttendanceSummaries { get; set; } = new List<StudentAttendanceSummary>();

    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
