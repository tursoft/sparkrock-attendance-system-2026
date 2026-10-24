#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class StudentAlert
{
    public int AlertId { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public string AlertType { get; set; } = null!;

    public string SchoolYear { get; set; } = null!;

    public DateTime AlertDate { get; set; }

    public string? AlertMessage { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public string? ResolvedBy { get; set; }

    public virtual School School { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
