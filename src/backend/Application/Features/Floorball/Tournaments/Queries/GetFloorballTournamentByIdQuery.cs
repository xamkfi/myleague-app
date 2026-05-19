using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for retrieving a floorball tournament by ID
/// </summary>
public record GetFloorballTournamentByIdQuery(Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
