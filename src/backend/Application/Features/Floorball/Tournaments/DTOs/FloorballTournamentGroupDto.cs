namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for a tournament group
/// </summary>
/// <param name="Id">The unique identifier of the group</param>
/// <param name="Name">The name of the group (e.g., "A-Lohko")</param>
/// <param name="Order">The display order of this group within the tournament</param>
/// <param name="Teams">List of teams in this group</param>
public record FloorballTournamentGroupDto(
    Guid Id,
    string Name,
    int Order,
    List<FloorballTournamentGroupTeamDto> Teams);
