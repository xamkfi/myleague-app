using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record DeleteFloorballTournamentCommand(Guid Id) : IRequest<Result>;
