using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
namespace DomainTestProject.Hockey;

/// <summary>
/// Domain tests for inferring the hockey match result when finish omits it.
/// </summary>
public class HockeyMatchResultInferenceTests
{
    [Theory]
    [InlineData(3, 1, false, false, HockeyMatchResultType.HomeWin)]
    [InlineData(1, 4, false, false, HockeyMatchResultType.AwayWin)]
    [InlineData(2, 2, false, false, HockeyMatchResultType.Draw)]
    [InlineData(3, 2, true, false, HockeyMatchResultType.OvertimeHomeWin)]
    [InlineData(2, 3, true, false, HockeyMatchResultType.OvertimeAwayWin)]
    [InlineData(4, 3, true, true, HockeyMatchResultType.ShootoutHomeWin)]
    [InlineData(3, 4, false, true, HockeyMatchResultType.ShootoutAwayWin)]
    public void MarkFinished_WithoutResultType_InfersFromScoreAndFlags(
        int homeGoals,
        int awayGoals,
        bool wentToOvertime,
        bool wentToShootout,
        HockeyMatchResultType expected)
    {
        HockeyMatch match = CreateMatch(homeGoals, awayGoals);
        match.SetWentToOvertime(wentToOvertime);
        match.SetWentToShootout(wentToShootout);

        match.MarkFinished();

        match.ResultType.Should().Be(expected);
    }

    [Fact]
    public void MarkFinished_WithExplicitResultType_KeepsIt()
    {
        HockeyMatch match = CreateMatch(3, 1);

        match.MarkFinished(resultType: HockeyMatchResultType.ForfeitHomeWin);

        match.ResultType.Should().Be(HockeyMatchResultType.ForfeitHomeWin);
    }

    [Fact]
    public void StandingResultType_InProgressMatch_IsNull()
    {
        HockeyMatch match = CreateMatch(1, 0);
        match.MarkStarted();

        match.StandingResultType.Should().BeNull();
    }

    private static HockeyMatch CreateMatch(int homeGoals, int awayGoals)
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Home");
        HockeyTeam awayTeam = HockeyTestHelpers.CreateTeam("Away");
        (HockeyMatch match, _, _) = HockeyTestHelpers.CreateMatchWithSides(homeTeam, awayTeam);
        match.SetTeamGoals(HockeyTeamSlot.Home, homeGoals);
        match.SetTeamGoals(HockeyTeamSlot.Away, awayGoals);
        return match;
    }
}
