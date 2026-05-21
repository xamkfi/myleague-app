using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// One slot in a tournament's pre-defined playoff schedule. The orchestrator-side import
/// shape mirrors the import JSON to keep validation simple; the handler converts each item
/// to a <see cref="Domain.ValueObjects.Floorball.PlayoffScheduleSlot"/> before saving.
/// </summary>
public record PlayoffScheduleSlotInput(
    FloorballPlayoffRound Round,
    int Order,
    DateTime ScheduledDateTime,
    string? Venue);

/// <summary>
/// Command for creating a floorball tournament
/// </summary>
public record CreateFloorballTournamentCommand(
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
