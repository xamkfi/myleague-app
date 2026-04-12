using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for deleting a floorball tournament
/// </summary>
public record DeleteFloorballTournamentCommand(
    Guid CompetitionId) : IRequest<Result>;
