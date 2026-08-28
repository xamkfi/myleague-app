using Application.Features.Football.Matches.DTOs;

namespace Application.Features.Football.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for FootballTournamentRules value object
/// </summary>
/// <param name="GroupStageMatchRules">Match rules for the group stage</param>
/// <param name="PlayoffMatchRules">Match rules for the playoff stage</param>
/// <param name="TeamsAdvancingPerGroup">Number of teams advancing from each group</param>
/// <param name="HasPlayoffStage">Whether the tournament includes a playoff stage</param>
/// <param name="HasThirdPlaceMatch">Whether the tournament includes a third-place match</param>
public record FootballTournamentRulesDto(
    FootballMatchRulesDto GroupStageMatchRules,
    FootballMatchRulesDto PlayoffMatchRules,
    int TeamsAdvancingPerGroup,
    bool HasPlayoffStage,
    bool HasThirdPlaceMatch);
