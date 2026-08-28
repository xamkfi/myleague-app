using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Queries;

/// <summary>
/// Query for retrieving all active football tournaments
/// </summary>
public record GetActiveFootballTournamentsQuery(
    Domain.Enums.Common.TeamCategory? TeamCategory = null
) : IRequest<Result<List<FootballTournamentDto>>>;
