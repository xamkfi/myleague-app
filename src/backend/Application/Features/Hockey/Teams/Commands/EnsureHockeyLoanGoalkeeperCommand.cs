using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Creates the team's loan goalkeeper, or returns the existing one and ensures a roster
/// row for <paramref name="CompetitionId"/>.
/// </summary>
public record EnsureHockeyLoanGoalkeeperCommand(
    Guid TeamId,
    Guid? CompetitionId) : IRequest<Result<HockeyLoanGoalkeeperDto>>;
