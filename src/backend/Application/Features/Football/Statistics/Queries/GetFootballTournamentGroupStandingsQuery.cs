using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving standings for a single tournament group.
/// </summary>
public record GetFootballTournamentGroupStandingsQuery(Guid GroupId)
    : IRequest<Result<List<FootballTournamentGroupStandingDto>>>;
