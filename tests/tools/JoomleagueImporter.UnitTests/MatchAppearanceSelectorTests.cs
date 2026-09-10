using JoomleagueImporter.Import;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.UnitTests;

public class MatchAppearanceSelectorTests
{
    [Fact]
    public void PlayerIdsOnSide_UsesMatchPlayerRows_NotWholeRoster()
    {
        MatchImport match = new()
        {
            Match = new OldMatch { Id = 1, ProjectTeam1Id = 1, ProjectTeam2Id = 2 },
            Events = [],
            Players =
            [
                new OldMatchPlayer { MatchId = 1, TeamPlayerId = 11 },
            ],
        };
        Dictionary<int, Guid> map = new()
        {
            [11] = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            [12] = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        };

        HashSet<Guid> appeared = MatchAppearanceSelector.PlayerIdsOnSide(
            match, map, [11, 12], []);

        appeared.Should().Equal(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    }

    [Fact]
    public void PlayerIdsOnSide_WithoutAppearances_UsesEventsOnly()
    {
        MatchImport match = new()
        {
            Match = new OldMatch { Id = 2, ProjectTeam1Id = 1, ProjectTeam2Id = 2 },
            Events = [],
            Players = [],
        };
        Dictionary<int, Guid> map = new()
        {
            [11] = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            [12] = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        };
        Guid scorer = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        HashSet<Guid> appeared = MatchAppearanceSelector.PlayerIdsOnSide(
            match, map, [11, 12], [scorer]);

        appeared.Should().Equal(scorer);
    }
}
