using Application.Features.Floorball.Tournaments.DTOs;
using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record ChangeFloorballTournamentStatusCommand(
    Guid Id,
    string Action) : IRequest<Result<FloorballTournamentDto>>;
