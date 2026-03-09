using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record RemoveGroupFromTournamentCommand(
    Guid TournamentId,
    Guid GroupId) : IRequest<Result>;
