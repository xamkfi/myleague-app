using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Teams.Commands;

/// <summary>
/// Creates the team's loan goalkeeper, or returns the existing one and ensures a roster
/// row for <paramref name="CompetitionId"/>.
/// </summary>
public record EnsureFootballLoanGoalkeeperCommand(
    Guid TeamId,
    Guid? CompetitionId) : IRequest<Result<FootballLoanGoalkeeperDto>>;
