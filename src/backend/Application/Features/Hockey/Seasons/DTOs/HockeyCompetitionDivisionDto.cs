namespace Application.Features.Hockey.Seasons.DTOs;

/// <summary>
/// Data transfer object for a hockey competition division (season division link).
/// </summary>
public record HockeyCompetitionDivisionDto(
    Guid Id,
    Guid CompetitionId,
    Guid DivisionId,
    string Name,
    int SortOrder,
    bool IsActive,
    Guid? ChampionCompetitionTeamId,
    IReadOnlyCollection<HockeyCompetitionDivisionTeamDto> Teams);

/// <summary>
/// Data transfer object for a team membership within a hockey competition division.
/// </summary>
public record HockeyCompetitionDivisionTeamDto(
    Guid Id,
    Guid CompetitionDivisionId,
    Guid CompetitionTeamId,
    int? Seed,
    bool IsActive);
