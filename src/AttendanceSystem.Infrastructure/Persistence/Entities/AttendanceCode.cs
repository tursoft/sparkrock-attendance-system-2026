#nullable enable

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Infrastructure.Persistence.Entities;

public partial class AttendanceCode
{
    public int CodeId { get; set; }

    public string CodeValue { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsAbsent { get; set; }

    public bool IsExcused { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();
}
