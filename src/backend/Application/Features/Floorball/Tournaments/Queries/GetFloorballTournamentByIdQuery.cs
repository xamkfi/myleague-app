using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for retrieving a floorball tournament by its unique identifier
/// </summary>
/// <param name="Id">The tournament ID</param>
public record GetFloorballTournamentByIdQuery(Guid Id) : IRequest<Result<FloorballTournamentDto>>;
