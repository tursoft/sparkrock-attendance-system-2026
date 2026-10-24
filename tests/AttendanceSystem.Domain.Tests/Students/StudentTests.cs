using AttendanceSystem.Domain.Students;

namespace AttendanceSystem.Domain.Tests.Students;

public class StudentTests
{
    [Fact]
    public void Keeps_its_details()
    {
        var student = new Student(12, 3, true);

        Assert.Equal(12, student.Id);
        Assert.Equal(3, student.SchoolId);
        Assert.True(student.IsActive);
    }

    [Fact]
    public void An_active_student_is_enrolled_at_their_school()
    {
        Assert.True(new Student(12, 3, true).IsEnrolledAt(3));
    }

    [Fact]
    public void A_student_is_not_enrolled_at_another_school()
    {
        Assert.False(new Student(12, 3, true).IsEnrolledAt(4));
    }

    [Fact]
    public void An_inactive_student_is_not_enrolled()
    {
        Assert.False(new Student(12, 3, false).IsEnrolledAt(3));
    }
}
