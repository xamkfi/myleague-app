using Domain.Common;

namespace DomainTestProject.Common;

public class PlayerLicenceCalendarTests
{
    [Fact]
    public void ResolveCutoff_February29InLeapYear_Keeps29()
    {
        DateOnly cutoff = PlayerLicenceCalendar.ResolveCutoff(2024, 2, 29);

        cutoff.Should().Be(new DateOnly(2024, 2, 29));
    }

    [Fact]
    public void ResolveCutoff_February29InNonLeapYear_FallsBackTo28()
    {
        DateOnly cutoff = PlayerLicenceCalendar.ResolveCutoff(2025, 2, 29);

        cutoff.Should().Be(new DateOnly(2025, 2, 28));
    }

    [Fact]
    public void CurrentCutoffYear_OnOrAfterCutoff_ReturnsThisYear()
    {
        int year = PlayerLicenceCalendar.CurrentCutoffYear(new DateOnly(2026, 5, 1), 5, 1);

        year.Should().Be(2026);
    }

    [Fact]
    public void CurrentCutoffYear_BeforeCutoff_ReturnsPreviousYear()
    {
        int year = PlayerLicenceCalendar.CurrentCutoffYear(new DateOnly(2026, 4, 30), 5, 1);

        year.Should().Be(2025);
    }

    [Fact]
    public void IsAutomaticResetDue_NullLastYear_IsFalse()
    {
        bool due = PlayerLicenceCalendar.IsAutomaticResetDue(new DateOnly(2026, 9, 21), 5, 1, null);

        due.Should().BeFalse();
    }

    [Fact]
    public void IsAutomaticResetDue_BeforeCutoff_IsFalse()
    {
        bool due = PlayerLicenceCalendar.IsAutomaticResetDue(new DateOnly(2026, 4, 30), 5, 1, 2025);

        due.Should().BeFalse();
    }

    [Fact]
    public void IsAutomaticResetDue_AfterCutoffAndDifferentYear_IsTrue()
    {
        bool due = PlayerLicenceCalendar.IsAutomaticResetDue(new DateOnly(2026, 5, 1), 5, 1, 2025);

        due.Should().BeTrue();
    }

    [Fact]
    public void IsAutomaticResetDue_AlreadyRunThisYear_IsFalse()
    {
        bool due = PlayerLicenceCalendar.IsAutomaticResetDue(new DateOnly(2026, 9, 21), 5, 1, 2026);

        due.Should().BeFalse();
    }

    [Fact]
    public void TodayInHelsinki_UsesHelsinkiOffset()
    {
        DateTime utc = new(2026, 4, 30, 21, 0, 0, DateTimeKind.Utc);

        DateOnly today = PlayerLicenceCalendar.TodayInHelsinki(utc);

        today.Should().Be(new DateOnly(2026, 5, 1));
    }
}
