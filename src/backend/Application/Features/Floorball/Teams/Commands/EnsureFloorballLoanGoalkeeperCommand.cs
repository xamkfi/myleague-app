using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands;

/// <summary>
/// Creates the team's loan goalkeeper, or returns the existing one and ensures a roster
/// row for <paramref name="CompetitionId"/>.
/// </summary>
public record EnsureFloorballLoanGoalkeeperCommand(
    Guid TeamId,
    Guid? CompetitionId) : IRequest<Result<FloorballLoanGoalkeeperDto>>;
