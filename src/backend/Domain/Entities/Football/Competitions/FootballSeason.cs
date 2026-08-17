using Domain.Enums.Common;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// A football league season.
/// </summary>
public class FootballSeason : FootballCompetition
{
    private FootballSeason() : base() { }

    public FootballSeason(
        string name,
        DateTime startDate,
        DateTime endDate,
        FootballMatchRules? matchRules = null,
        FootballStandingRules? standingRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
        : base(name, startDate, endDate, matchRules, standingRules, teamCategory) { }
}
