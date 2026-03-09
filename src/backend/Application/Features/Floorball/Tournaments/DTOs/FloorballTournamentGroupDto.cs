namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for a group within a floorball tournament
/// </summary>
/// <param name="Id">The unique identifier of the group</param>
/// <param name="TournamentId">The tournament this group belongs to</param>
/// <param name="Name">The name of the group (e.g., "A-lohko")</param>
/// <param name="Phase">The phase this group belongs to (GroupStage or Playoff)</param>
/// <param name="SortOrder">Display sort order</param>
/// <param name="Teams">Teams in this group</param>
public record FloorballTournamentGroupDto(
    Guid Id,
    Guid TournamentId,
    string Name,
    string Phase,
    int SortOrder,
    IReadOnlyCollection<FloorballTournamentGroupTeamDto> Teams);
