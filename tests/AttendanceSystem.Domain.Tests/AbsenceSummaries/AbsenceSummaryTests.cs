using AttendanceSystem.Domain.AbsenceSummaries;
using AttendanceSystem.Domain.Schools;

namespace AttendanceSystem.Domain.Tests.AbsenceSummaries;

public class AbsenceSummaryTests
{
    private const int Threshold = 10;

    private static readonly SchoolYear Year = SchoolYear.Parse("2025-2026");
    private static readonly DateTime StartedAt = new(2025, 9, 15, 13, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = new(2026, 2, 3, 15, 45, 0, DateTimeKind.Utc);
    private static readonly DateTime EarlierAlertDate = new(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc);

    private static AbsenceSummary Started() => AbsenceSummary.Start(12, 3, Year, StartedAt);

    private static AbsenceSummary Restored(int totalAbsences, ChronicAbsenceAlert? openAlert = null) =>
        AbsenceSummary.Restore(
            id: 7,
            studentId: 12,
            schoolId: 3,
            schoolYear: Year,
            totalAbsences: totalAbsences,
            lastUpdated: StartedAt,
            openAlert: openAlert);

    private static ChronicAbsenceAlert ExistingAlert() =>
        ChronicAbsenceAlert.Restore(21, EarlierAlertDate, "Student has reached 10 absences this year");

    [Fact]
    public void Start_creates_an_unsaved_summary_with_no_absences()
    {
        var summary = Started();

        Assert.Equal(0, summary.Id);
        Assert.Equal(12, summary.StudentId);
        Assert.Equal(3, summary.SchoolId);
        Assert.Equal(Year, summary.SchoolYear);
        Assert.Equal(0, summary.TotalAbsences);
        Assert.Equal(StartedAt, summary.LastUpdated);
        Assert.Null(summary.OpenAlert);
        Assert.Null(summary.RaisedAlert);
    }

    [Fact]
    public void Restore_sets_every_persisted_field_including_the_open_alert()
    {
        var alert = ExistingAlert();

        var summary = Restored(totalAbsences: 11, openAlert: alert);

        Assert.Equal(7, summary.Id);
        Assert.Equal(12, summary.StudentId);
        Assert.Equal(3, summary.SchoolId);
        Assert.Equal(Year, summary.SchoolYear);
        Assert.Equal(11, summary.TotalAbsences);
        Assert.Equal(StartedAt, summary.LastUpdated);
        Assert.Same(alert, summary.OpenAlert);
        Assert.Null(summary.RaisedAlert);
    }

    [Fact]
    public void Recalculate_below_the_threshold_updates_the_total_without_an_alert()
    {
        var summary = Started();

        var raised = summary.Recalculate(9, Threshold, Now);

        Assert.False(raised);
        Assert.Equal(9, summary.TotalAbsences);
        Assert.Equal(Now, summary.LastUpdated);
        Assert.Null(summary.OpenAlert);
        Assert.Null(summary.RaisedAlert);
    }

    [Fact]
    public void Recalculate_at_the_threshold_raises_a_chronic_absence_alert()
    {
        var summary = Started();

        var raised = summary.Recalculate(10, Threshold, Now);

        Assert.True(raised);
        Assert.Equal(10, summary.TotalAbsences);
        var alert = Assert.IsType<ChronicAbsenceAlert>(summary.RaisedAlert);
        Assert.Same(alert, summary.OpenAlert);
        Assert.Equal(0, alert.Id);
        Assert.Equal("CHRONIC_ABSENCE", alert.AlertType);
        Assert.Equal(Now, alert.AlertDate);
        Assert.Equal("Student has reached 10 absences this year", alert.Message);
    }

    [Fact]
    public void Recalculate_above_the_threshold_without_an_open_alert_raises_one_with_the_total()
    {
        var summary = Restored(totalAbsences: 8);

        var raised = summary.Recalculate(14, Threshold, Now);

        Assert.True(raised);
        Assert.Equal("Student has reached 14 absences this year", summary.RaisedAlert?.Message);
    }

    [Theory]
    [InlineData(2, 3, false)]
    [InlineData(3, 3, true)]
    [InlineData(4, 3, true)]
    [InlineData(19, 20, false)]
    [InlineData(20, 20, true)]
    public void Recalculate_uses_the_given_threshold(int totalAbsences, int threshold, bool expectedRaised)
    {
        Assert.Equal(expectedRaised, Started().Recalculate(totalAbsences, threshold, Now));
    }

    [Fact]
    public void Recalculate_with_an_open_alert_raises_no_new_alert()
    {
        var alert = ExistingAlert();
        var summary = Restored(totalAbsences: 11, openAlert: alert);

        var raised = summary.Recalculate(12, Threshold, Now);

        Assert.False(raised);
        Assert.Equal(12, summary.TotalAbsences);
        Assert.Equal(Now, summary.LastUpdated);
        Assert.Same(alert, summary.OpenAlert);
        Assert.Null(summary.RaisedAlert);
    }

    [Fact]
    public void Recalculate_raises_at_most_one_alert()
    {
        var summary = Started();
        summary.Recalculate(10, Threshold, StartedAt);
        var first = summary.RaisedAlert;

        var raisedAgain = summary.Recalculate(11, Threshold, Now);

        Assert.False(raisedAgain);
        Assert.Same(first, summary.RaisedAlert);
        Assert.Same(first, summary.OpenAlert);
        Assert.Equal(11, summary.TotalAbsences);
    }

    [Fact]
    public void An_open_alert_stays_open_when_the_total_drops_below_the_threshold()
    {
        var alert = ExistingAlert();
        var summary = Restored(totalAbsences: 10, openAlert: alert);

        var raised = summary.Recalculate(9, Threshold, Now);

        Assert.False(raised);
        Assert.Equal(9, summary.TotalAbsences);
        Assert.Same(alert, summary.OpenAlert);
    }

    [Fact]
    public void Recalculate_after_the_previous_alert_was_resolved_raises_a_new_one()
    {
        var summary = Restored(totalAbsences: 12, openAlert: null);

        var raised = summary.Recalculate(12, Threshold, Now);

        Assert.True(raised);
        Assert.Equal("Student has reached 12 absences this year", summary.RaisedAlert?.Message);
    }

    [Fact]
    public void Recalculate_rejects_a_negative_total()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Started().Recalculate(-1, Threshold, Now));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(11, true)]
    public void A_student_is_chronically_absent_at_or_above_the_threshold(int totalAbsences, bool expected)
    {
        Assert.Equal(expected, Restored(totalAbsences).IsChronicallyAbsent(Threshold));
    }
}
