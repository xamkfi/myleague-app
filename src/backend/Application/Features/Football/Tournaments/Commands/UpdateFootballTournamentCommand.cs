using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for updating a football tournament
/// </summary>
public record UpdateFootballTournamentCommand(
    Guid CompetitionId,
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
    Domain.Enums.Common.TeamCategory? TeamCategory = null) : IRequest<Result<FootballTournamentDto>>;
