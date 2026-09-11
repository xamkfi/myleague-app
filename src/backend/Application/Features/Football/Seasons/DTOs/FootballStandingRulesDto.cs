namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Point allocation for football standings.
/// </summary>
public record FootballStandingRulesDto(
    int WinPoints,
    int DrawPoints,
    int LossPoints);
