#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Grade { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual School School { get; set; } = null!;

    public virtual ICollection<StudentAlert> StudentAlerts { get; set; } = new List<StudentAlert>();

    public virtual ICollection<StudentAttendanceSummary> StudentAttendanceSummaries { get; set; } = new List<StudentAttendanceSummary>();

    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();
}
