#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class StudentAttendance
{
    public int AttendanceId { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public DateOnly AttendDate { get; set; }

    public int? TermId { get; set; }

    public string AttendCode { get; set; } = null!;

    public bool IsAbsent { get; set; }

    public bool IsExcused { get; set; }

    public int? MinutesLate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual AttendanceCode AttendCodeNavigation { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual SchoolTerm? Term { get; set; }
}
