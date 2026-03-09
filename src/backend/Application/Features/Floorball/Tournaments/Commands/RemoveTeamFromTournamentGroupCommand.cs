using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record RemoveTeamFromTournamentGroupCommand(
    Guid TournamentId,
    Guid GroupId,
    Guid TeamId) : IRequest<Result>;
