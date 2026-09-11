using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to assign home/away competition teams to a playoff series.
/// </summary>
public record AssignHockeyPlayoffSeriesTeamsCommand(
    Guid TournamentId,
    Guid SeriesId,
    Guid HomeCompetitionTeamId,
    Guid AwayCompetitionTeamId) : IRequest<Result<HockeyTournamentDto>>;
