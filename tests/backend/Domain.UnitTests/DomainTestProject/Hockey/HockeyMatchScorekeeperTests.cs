using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;

namespace DomainTestProject.Hockey;

/// <summary>
/// Domain tests for optional hockey match scorekeepers (toimitsijat).
/// </summary>
public class HockeyMatchScorekeeperTests
{
    [Fact]
    public void AddScorekeeper_SamePersonTwice_KeepsSingleEntry()
    {
        HockeyMatch match = HockeyTestHelpers.CreateStandaloneMatch();
        Guid personId = Guid.NewGuid();

        match.AddScorekeeper(personId);
        match.AddScorekeeper(personId);

        match.Scorekeepers.Should().ContainSingle(s => s.PersonId == personId);
    }

    [Fact]
    public void RemoveScorekeeper_LastScorekeeper_LeavesEmptyList()
    {
        HockeyMatch match = HockeyTestHelpers.CreateStandaloneMatch();
        Guid personId = Guid.NewGuid();
        match.AddScorekeeper(personId);

        match.RemoveScorekeeper(personId);

        match.Scorekeepers.Should().BeEmpty();
    }

    [Fact]
    public void AddScorekeeper_FinishedMatch_Throws()
    {
        HockeyMatch match = HockeyTestHelpers.CreateStandaloneMatch();
        match.SetStatus(HockeyMatchStatus.Finished);

        Action act = () => match.AddScorekeeper(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
