using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command for creating a hockey season.
/// </summary>
/// <param name="Name">Name of the season</param>
/// <param name="StartDate">Season start date</param>
/// <param name="EndDate">Season end date</param>
/// <param name="SeasonCode">Optional short season code</param>
public record CreateHockeySeasonCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? SeasonCode = null,
    TeamCategory TeamCategory = TeamCategory.Adult,
    string? LogoUrl = null,
    int TeamsAdvancing = 0,
    IReadOnlyList<StandingSortCriterion>? RankingCriteria = null,
    int RegulationWinPoints = 3,
    int OvertimeWinPoints = 2,
    int ShootoutWinPoints = 2,
    int OvertimeLossPoints = 1,
    int ShootoutLossPoints = 1,
    int TiePoints = 1) : IRequest<Result<HockeySeasonDto>>;
