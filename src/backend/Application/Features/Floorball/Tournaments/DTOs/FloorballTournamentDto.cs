using Application.Features.Floorball.Matches.DTOs;

namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Full Data Transfer Object for FloorballTournament entity
/// </summary>
/// <param name="Id">The unique identifier of the tournament</param>
/// <param name="Name">The name of the tournament (e.g., "Duuniturnaus 2025")</param>
/// <param name="DescriptionHtml">Rich HTML description of the tournament</param>
/// <param name="StartDate">The start date of the tournament</param>
/// <param name="EndDate">The end date of the tournament</param>
/// <param name="Location">The location/venue of the tournament</param>
/// <param name="Status">The lifecycle status of the tournament</param>
/// <param name="PlayoffFormat">The playoff format after group stage</param>
/// <param name="GroupStageAdvancingCount">Number of teams advancing from each group</param>
/// <param name="ImageUrls">URLs of images associated with the tournament description</param>
/// <param name="MatchRules">Match rules configuration for this tournament</param>
/// <param name="Groups">Groups in this tournament with their teams</param>
/// <param name="Matches">Matches scheduled for this tournament</param>
public record FloorballTournamentDto(
    Guid Id,
    string Name,
    string? DescriptionHtml,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    string Status,
    string PlayoffFormat,
    int GroupStageAdvancingCount,
    IReadOnlyCollection<string> ImageUrls,
    FloorballMatchRulesDto MatchRules,
    IReadOnlyCollection<FloorballTournamentGroupDto> Groups,
    IReadOnlyCollection<FloorballMatchDto> Matches);
