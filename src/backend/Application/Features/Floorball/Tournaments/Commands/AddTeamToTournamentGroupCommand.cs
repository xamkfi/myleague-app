using Application.Features.Floorball.Tournaments.DTOs;
using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record AddTeamToTournamentGroupCommand(
    Guid TournamentId,
    Guid GroupId,
    Guid TeamId) : IRequest<Result<FloorballTournamentGroupTeamDto>>;
