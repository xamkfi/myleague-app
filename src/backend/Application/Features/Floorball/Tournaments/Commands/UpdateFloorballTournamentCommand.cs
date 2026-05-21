using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for updating a floorball tournament
/// </summary>
public record UpdateFloorballTournamentCommand(
    Guid CompetitionId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Venue,
    string? ContentHtml,
    int GroupStageNumberOfPeriods,
    int GroupStagePeriodDurationMinutes,
    bool GroupStageAllowOvertime,
    int GroupStageOvertimeDurationMinutes,
    bool GroupStageAllowShootout,
    int PlayoffNumberOfPeriods,
    int PlayoffPeriodDurationMinutes,
    bool PlayoffAllowOvertime,
    int PlayoffOvertimeDurationMinutes,
    bool PlayoffAllowShootout,
    int TeamsAdvancingPerGroup,
    bool HasPlayoffStage,
    bool HasThirdPlaceMatch,
    IReadOnlyList<PlayoffScheduleSlotInput>? PlayoffSchedule = null) : IRequest<Result<FloorballTournamentDto>>;
