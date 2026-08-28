namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Data Transfer Object for a division within a football season.
/// </summary>
public record FootballSeasonDivisionDto(
    Guid DivisionId,
    int TeamCount,
    IReadOnlyCollection<Guid> TeamIds);
