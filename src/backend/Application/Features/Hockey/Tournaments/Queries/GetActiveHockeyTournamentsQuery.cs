using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

/// <summary>
/// Query to get active hockey tournaments.
/// </summary>
public record GetActiveHockeyTournamentsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeyTournamentDto>>>;
