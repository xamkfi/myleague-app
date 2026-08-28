namespace Application.Features.Floorball.Seasons.DTOs;

/// <summary>
/// Season-year option for public year navigation.
/// </summary>
public record FloorballSeasonYearDto(
    string Year,
    int SeasonCount,
    bool HasActiveSeason);
