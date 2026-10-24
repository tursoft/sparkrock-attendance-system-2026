using AttendanceSystem.Domain.Attendance;

namespace AttendanceSystem.Domain.Tests.Attendance;

public class AttendanceCodeTests
{
    [Fact]
    public void Keeps_its_details()
    {
        var code = new AttendanceCode("E", "Excused absence", isAbsent: true, isExcused: true, isActive: false);

        Assert.Equal("E", code.Value);
        Assert.Equal("Excused absence", code.Description);
        Assert.True(code.IsAbsent);
        Assert.True(code.IsExcused);
        Assert.False(code.IsActive);
    }
}
