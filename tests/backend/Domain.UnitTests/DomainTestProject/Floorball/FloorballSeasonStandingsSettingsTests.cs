using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Football.Competitions;
using Domain.Enums.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Services.Common;
using Domain.ValueObjects.Hockey.Rules;

namespace DomainTestProject.Floorball;

public class SeasonStandingsSettingsTests
{
    [Fact]
    public void FloorballSeason_NewSeason_UsesDefaultStandingsSettings()
    {
        FloorballSeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));

        season.TeamsAdvancing.Should().Be(0);
        season.RankingCriteria.Should().Equal(StandingSortCriteria.Default);
    }

    [Fact]
    public void FloorballSeason_UpdateStandingsSettings_StoresTheGivenValues()
    {
        FloorballSeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));
        List<StandingSortCriterion> criteria =
        [
            StandingSortCriterion.GoalsAgainst,
            StandingSortCriterion.Points
        ];

        season.UpdateStandingsSettings(4, criteria);

        season.TeamsAdvancing.Should().Be(4);
        season.RankingCriteria.Should().Equal(criteria);
    }

    [Fact]
    public void FootballSeason_UpdateStandingsSettings_RejectsANegativeCount()
    {
        FootballSeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));

        Action act = () => season.UpdateStandingsSettings(-1, null);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("teamsAdvancing");
    }

    [Fact]
    public void FloorballSeason_UpdateStandingsSettings_RejectsDuplicateCriteria()
    {
        FloorballSeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));

        Action act = () => season.UpdateStandingsSettings(1,
        [
            StandingSortCriterion.Points,
            StandingSortCriterion.Points
        ]);

        act.Should().Throw<ArgumentException>().WithParameterName("criteria");
    }

    [Fact]
    public void HockeySeason_NewSeason_UsesTheDefaultRankingAndKeepsPointRules()
    {
        HockeySeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));

        season.TeamsAdvancing.Should().Be(0);
        season.RankingCriteria.Should().Equal(StandingSortCriteria.Default);
        season.CompetitionRules.StandingRules.TieBreakers.Should().Equal(HockeyStandingRules.Default().TieBreakers);
    }

    [Fact]
    public void HockeySeason_UpdateStandingsSettings_StoresRankingWithoutChangingPointRules()
    {
        HockeySeason season = new("2026-2027", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6));
        List<StandingSortCriterion> criteria =
        [
            StandingSortCriterion.GoalsFor,
            StandingSortCriterion.Points
        ];

        season.UpdateStandingsSettings(2, criteria);

        season.TeamsAdvancing.Should().Be(2);
        season.RankingCriteria.Should().Equal(criteria);
        season.CompetitionRules.StandingRules.TieBreakers.Should().Equal(HockeyStandingRules.Default().TieBreakers);
        season.CompetitionRules.StandingRules.RegulationWinPoints.Should().Be(HockeyStandingRules.Default().RegulationWinPoints);
    }
}
