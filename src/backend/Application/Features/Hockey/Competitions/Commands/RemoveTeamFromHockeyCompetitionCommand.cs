using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to soft-remove a team from a hockey competition.
/// </summary>
public record RemoveTeamFromHockeyCompetitionCommand(
    Guid CompetitionId,
    Guid TeamId) : IRequest<Result<HockeyCompetitionDto>>;
