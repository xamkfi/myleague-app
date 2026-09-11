namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Season-year option for public year navigation.
/// </summary>
public record FootballSeasonYearDto(
    string Year,
    int SeasonCount,
    bool HasActiveSeason);
