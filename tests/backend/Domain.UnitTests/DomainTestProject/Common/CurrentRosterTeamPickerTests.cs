using Domain.Common;

namespace DomainTestProject.Common;

public class CurrentRosterTeamPickerTests
{
    private static readonly Guid OlderTeam = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LatestTeam = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void PickTeamId_UsesLatestCompetition_WhenAnOlderMembershipIsStillActive()
    {
        Guid? teamId = CurrentRosterTeamPicker.PickTeamId(
        [
            new RosterMembershipCandidate(
                OlderTeam,
                new DateTime(2020, 4, 1),
                new DateTime(2019, 9, 1),
                false,
                true,
                new DateTime(2026, 1, 2)),
            new RosterMembershipCandidate(
                LatestTeam,
                new DateTime(2026, 4, 1),
                new DateTime(2025, 9, 1),
                false,
                false,
                new DateTime(2026, 1, 1)),
        ]);

        teamId.Should().Be(LatestTeam);
    }

    [Fact]
    public void PickTeamId_PrefersCurrentCompetition_WhenEndDatesMatch()
    {
        DateTime end = new(2026, 5, 1);
        Guid? teamId = CurrentRosterTeamPicker.PickTeamId(
        [
            new RosterMembershipCandidate(OlderTeam, end, new DateTime(2025, 9, 1), false, true, end),
            new RosterMembershipCandidate(LatestTeam, end, new DateTime(2025, 9, 1), true, false, end),
        ]);

        teamId.Should().Be(LatestTeam);
    }
}
