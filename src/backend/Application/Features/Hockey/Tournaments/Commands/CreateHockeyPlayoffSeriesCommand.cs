using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Hockey.Competitions;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to create a playoff series on a hockey tournament.
/// </summary>
public record CreateHockeyPlayoffSeriesCommand(
    Guid TournamentId,
    HockeyPlayoffRound Round,
    int SeriesOrder,
    int BestOf,
    Guid? HomeCompetitionTeamId = null,
    Guid? AwayCompetitionTeamId = null) : IRequest<Result<HockeyTournamentDto>>;
