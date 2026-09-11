using Application.Common;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for deleting a football tournament
/// </summary>
public record DeleteFootballTournamentCommand(
    Guid CompetitionId) : IRequest<Result>;
