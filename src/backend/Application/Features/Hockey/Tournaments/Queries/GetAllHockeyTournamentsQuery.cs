using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

/// <summary>
/// Query for retrieving all hockey tournaments.
/// </summary>
public record GetAllHockeyTournamentsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeyTournamentDto>>>;
