using Application.Features.Floorball.Matches.DTOs;

namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for FloorballTournamentRules value object
/// </summary>
/// <param name="GroupStageMatchRules">Match rules for the group stage</param>
/// <param name="PlayoffMatchRules">Match rules for the playoff stage</param>
/// <param name="TeamsAdvancingPerGroup">Number of teams advancing from each group</param>
/// <param name="HasPlayoffStage">Whether the tournament includes a playoff stage</param>
/// <param name="HasThirdPlaceMatch">Whether the tournament includes a third-place match</param>
public record FloorballTournamentRulesDto(
    FloorballMatchRulesDto GroupStageMatchRules,
    FloorballMatchRulesDto PlayoffMatchRules,
    int TeamsAdvancingPerGroup,
    bool HasPlayoffStage,
    bool HasThirdPlaceMatch);
