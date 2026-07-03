using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a hockey league season (e.g. "2024-2025").
/// </summary>
public class HockeySeason : HockeyCompetition
{
    public string? SeasonCode { get; private set; }
    public Guid? ChampionCompetitionTeamId { get; private set; }

    private HockeySeason() : base() { }

    public HockeySeason(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? seasonCode = null,
        HockeyCompetitionRules? competitionRules = null)
        : base(HockeyCompetitionType.Season, name, startDate, endDate, competitionRules)
    {
        SeasonCode = seasonCode;
    }

    public void UpdateSeasonCode(string? seasonCode) => SeasonCode = seasonCode;

    public void SetChampion(Guid championCompetitionTeamId)
    {
        if (championCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Champion competition team id cannot be empty.", nameof(championCompetitionTeamId));
        if (Status != HockeyCompetitionStatus.Completed)
            throw new InvalidOperationException("Champion can only be set for a completed season.");

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }
}
