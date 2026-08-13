namespace Application.Features.Football.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for a team within a tournament group
/// </summary>
/// <param name="Id">The unique identifier of the group-team link</param>
/// <param name="TeamId">The team ID</param>
/// <param name="TeamName">The name of the team</param>
public record FootballTournamentGroupTeamDto(
    Guid Id,
    Guid TeamId,
    string TeamName);
