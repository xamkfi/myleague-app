namespace Application.Features.Hockey.Tournaments.DTOs;

/// <summary>
/// Data transfer object for a hockey tournament group (lohko).
/// </summary>
/// <param name="Id">Unique identifier of the group</param>
/// <param name="TournamentId">Parent tournament id</param>
/// <param name="Name">Display name of the group</param>
/// <param name="SortOrder">Display order within the tournament</param>
/// <param name="Teams">Competition-team memberships in this group</param>
public record HockeyTournamentGroupDto(
    Guid Id,
    Guid TournamentId,
    string Name,
    int SortOrder,
    IReadOnlyCollection<HockeyTournamentGroupTeamDto> Teams);

/// <summary>
/// Data transfer object for a competition team placed in a tournament group.
/// </summary>
/// <param name="Id">Unique identifier of the group-team link</param>
/// <param name="TournamentGroupId">Group this membership belongs to</param>
/// <param name="CompetitionTeamId">Competition-team id (not raw HockeyTeam id)</param>
/// <param name="Seed">Optional seed within the group</param>
/// <param name="IsActive">Whether the membership is still active</param>
public record HockeyTournamentGroupTeamDto(
    Guid Id,
    Guid TournamentGroupId,
    Guid CompetitionTeamId,
    int? Seed,
    bool IsActive);
