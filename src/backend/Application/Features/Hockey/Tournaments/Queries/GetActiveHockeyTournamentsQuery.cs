using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

/// <summary>
/// Query to get active hockey tournaments.
/// </summary>
public record GetActiveHockeyTournamentsQuery() : IRequest<Result<IEnumerable<HockeyTournamentDto>>>;
