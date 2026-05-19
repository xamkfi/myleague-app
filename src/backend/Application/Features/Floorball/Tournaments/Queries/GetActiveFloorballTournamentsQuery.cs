using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for retrieving all active floorball tournaments
/// </summary>
public record GetActiveFloorballTournamentsQuery() : IRequest<Result<List<FloorballTournamentDto>>>;
