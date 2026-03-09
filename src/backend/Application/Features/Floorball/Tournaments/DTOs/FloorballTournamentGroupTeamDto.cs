namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for a team membership within a tournament group
/// </summary>
/// <param name="Id">The unique identifier of the group-team link</param>
/// <param name="GroupId">The group this team belongs to</param>
/// <param name="TeamId">The team identifier</param>
/// <param name="TeamName">The display name of the team</param>
public record FloorballTournamentGroupTeamDto(
    Guid Id,
    Guid GroupId,
    Guid TeamId,
    string TeamName);
