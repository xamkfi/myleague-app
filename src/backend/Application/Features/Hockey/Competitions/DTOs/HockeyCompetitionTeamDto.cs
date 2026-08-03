namespace Application.Features.Hockey.Competitions.DTOs;

public record HockeyCompetitionTeamDto(
    Guid Id,
    Guid CompetitionId,
    Guid TeamId,
    int? Seed,
    DateTime JoinedAt,
    bool IsActive);
