using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to assign home/away teams to a season playoff series.
/// </summary>
public record AssignHockeySeasonPlayoffSeriesTeamsCommand(
    Guid SeasonId,
    Guid SeriesId,
    Guid HomeCompetitionTeamId,
    Guid AwayCompetitionTeamId) : IRequest<Result<HockeySeasonDto>>;
