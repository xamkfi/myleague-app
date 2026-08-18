using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Queries;

/// <summary>
/// Query for retrieving a football tournament by ID
/// </summary>
public record GetFootballTournamentByIdQuery(Guid CompetitionId) : IRequest<Result<FootballTournamentDto>>;
