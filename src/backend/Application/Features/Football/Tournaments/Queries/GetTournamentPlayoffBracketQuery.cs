using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Queries;

/// <summary>
/// Query that returns the full playoff bracket for a tournament.
/// </summary>
/// <param name="CompetitionId">The tournament id.</param>
public record GetTournamentPlayoffBracketQuery(Guid CompetitionId)
    : IRequest<Result<FootballPlayoffBracketDto>>;
