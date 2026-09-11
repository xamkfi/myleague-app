using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// One slot in a tournament's pre-defined playoff schedule. The orchestrator-side import
/// shape mirrors the import JSON to keep validation simple; the handler converts each item
/// to a <see cref="Domain.ValueObjects.Football.FootballPlayoffScheduleSlot"/> before saving.
/// </summary>
public record FootballPlayoffScheduleSlotInput(
    FootballPlayoffRound Round,
    int Order,
    DateTime ScheduledDateTime,
    string? Venue);

/// <summary>
/// Command for creating a football tournament
/// </summary>
public record CreateFootballTournamentCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Venue,
    string? ContentHtml,
    int GroupStageNumberOfHalves,
    int GroupStageHalfDurationMinutes,
    int GroupStagePlayersOnField,
    bool GroupStageRequireGoalkeeper,
    int GroupStageMaxSubstitutions,
    bool GroupStageRequireOfficialsToStart,
    bool GroupStageAllowExtraTime,
    int GroupStageExtraTimeHalfCount,
    int GroupStageExtraTimeHalfDurationMinutes,
    bool GroupStageAllowPenaltyShootout,
    int PlayoffNumberOfHalves,
    int PlayoffHalfDurationMinutes,
    int PlayoffPlayersOnField,
    bool PlayoffRequireGoalkeeper,
    int PlayoffMaxSubstitutions,
    bool PlayoffRequireOfficialsToStart,
    bool PlayoffAllowExtraTime,
    int PlayoffExtraTimeHalfCount,
    int PlayoffExtraTimeHalfDurationMinutes,
    bool PlayoffAllowPenaltyShootout,
    int TeamsAdvancingPerGroup,
    bool HasPlayoffStage,
    bool HasThirdPlaceMatch,
    IReadOnlyList<FootballPlayoffScheduleSlotInput>? PlayoffSchedule = null,
    Domain.Enums.Common.TeamCategory TeamCategory = Domain.Enums.Common.TeamCategory.Adult) : IRequest<Result<FootballTournamentDto>>;
