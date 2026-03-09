namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Lightweight Data Transfer Object for tournament list views.
/// Excludes matches, description HTML, and detailed group information.
/// </summary>
/// <param name="Id">The unique identifier of the tournament</param>
/// <param name="Name">The name of the tournament</param>
/// <param name="StartDate">The start date of the tournament</param>
/// <param name="EndDate">The end date of the tournament</param>
/// <param name="Location">The location/venue of the tournament</param>
/// <param name="Status">The lifecycle status of the tournament</param>
/// <param name="PlayoffFormat">The playoff format after group stage</param>
/// <param name="GroupCount">Total number of groups in the tournament</param>
/// <param name="TeamCount">Total number of teams across all groups</param>
public record FloorballTournamentSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    string Status,
    string PlayoffFormat,
    int GroupCount,
    int TeamCount);
