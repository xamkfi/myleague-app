using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for retrieving all floorball tournaments, optionally filtered by status
/// </summary>
/// <param name="Status">Optional status filter (e.g. "Draft", "Active", "InProgress", "Completed", "Cancelled")</param>
public record GetAllFloorballTournamentsQuery(
    string? Status = null) : IRequest<Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>>;
