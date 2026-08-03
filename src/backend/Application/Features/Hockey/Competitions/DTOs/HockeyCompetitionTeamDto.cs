namespace Application.Features.Hockey.Competitions.DTOs;

/// <summary>
/// Data transfer object for a team's membership in a hockey competition.
/// </summary>
/// <param name="Id">Unique identifier of the competition-team link</param>
/// <param name="CompetitionId">Competition this membership belongs to</param>
/// <param name="TeamId">Hockey team id</param>
/// <param name="Seed">Optional seeding/order within the competition</param>
/// <param name="JoinedAt">When the team joined</param>
/// <param name="IsActive">Whether the membership is still active</param>
public record HockeyCompetitionTeamDto(
    Guid Id,
    Guid CompetitionId,
    Guid TeamId,
    int? Seed,
    DateTime JoinedAt,
    bool IsActive);
