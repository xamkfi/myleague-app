using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for retrieving all floorball tournaments
/// </summary>
public record GetAllFloorballTournamentsQuery() : IRequest<Result<List<FloorballTournamentDto>>>;
