using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

/// <summary>
/// Command for creating a football season.
/// </summary>
public record CreateFootballSeasonCommand(
    string Name,
    IEnumerable<Guid> DivisionIds,
    DateTime StartDate,
    DateTime EndDate,
    int NumberOfHalves = 2,
    int HalfDurationMinutes = 45,
    int PlayersOnField = 11,
    bool RequireGoalkeeper = true,
    int MaxSubstitutions = 0,
    bool RequireOfficialsToStart = false,
    bool AllowExtraTime = false,
    int ExtraTimeHalfCount = 2,
    int ExtraTimeHalfDurationMinutes = 15,
    bool AllowPenaltyShootout = false,
    int WinPoints = 3,
    int DrawPoints = 1,
    int LossPoints = 0,
    TeamCategory TeamCategory = TeamCategory.Adult) : IRequest<Result<FootballSeasonDto>>;
