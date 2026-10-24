using AttendanceSystem.Domain.AbsenceSummaries;

namespace AttendanceSystem.Domain.Tests.AbsenceSummaries;

public class ChronicAbsenceAlertTests
{
    [Fact]
    public void Restore_sets_every_persisted_field()
    {
        var alertDate = new DateTime(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc);

        var alert = ChronicAbsenceAlert.Restore(21, alertDate, "Student has reached 10 absences this year");

        Assert.Equal(21, alert.Id);
        Assert.Equal(ChronicAbsenceAlert.TypeName, alert.AlertType);
        Assert.Equal("CHRONIC_ABSENCE", ChronicAbsenceAlert.TypeName);
        Assert.Equal(alertDate, alert.AlertDate);
        Assert.Equal("Student has reached 10 absences this year", alert.Message);
    }

    [Fact]
    public void Restore_accepts_a_missing_message()
    {
        var alert = ChronicAbsenceAlert.Restore(21, DateTime.UnixEpoch, null);

        Assert.Null(alert.Message);
    }
}
