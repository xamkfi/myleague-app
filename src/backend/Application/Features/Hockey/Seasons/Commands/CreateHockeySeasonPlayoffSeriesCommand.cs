using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Hockey.Competitions;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to create a playoff series on a hockey season.
/// </summary>
public record CreateHockeySeasonPlayoffSeriesCommand(
    Guid SeasonId,
    HockeyPlayoffRound Round,
    int SeriesOrder,
    int BestOf,
    Guid? HomeCompetitionTeamId = null,
    Guid? AwayCompetitionTeamId = null) : IRequest<Result<HockeySeasonDto>>;
