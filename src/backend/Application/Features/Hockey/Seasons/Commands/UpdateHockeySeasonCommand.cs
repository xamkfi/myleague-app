using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: UpdateHockeySeason.
/// </summary>
public record UpdateHockeySeasonCommand(
    Guid SeasonId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? SeasonCode,
    TeamCategory TeamCategory,
    string? LogoUrl = null,
    int TeamsAdvancing = 0,
    IReadOnlyList<StandingSortCriterion>? RankingCriteria = null,
    int RegulationWinPoints = 3,
    int OvertimeWinPoints = 2,
    int ShootoutWinPoints = 2,
    int OvertimeLossPoints = 1,
    int ShootoutLossPoints = 1,
    int TiePoints = 1) : IRequest<Result<HockeySeasonDto>>;
