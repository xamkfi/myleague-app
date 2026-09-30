namespace Application.Features.Floorball.Seasons.DTOs;

/// <summary>
/// Point allocation for a floorball season table.
/// </summary>
public record FloorballStandingRulesDto(
    int WinPoints,
    int DrawPoints,
    int OvertimeWinPoints,
    int OvertimeLossPoints);
