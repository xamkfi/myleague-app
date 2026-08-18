using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

/// <summary>
/// Query for retrieving all hockey tournaments.
/// </summary>
public record GetAllHockeyTournamentsQuery() : IRequest<Result<IEnumerable<HockeyTournamentDto>>>;
