using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to soft-remove a competition team from a division.
/// </summary>
public record RemoveTeamFromHockeyCompetitionDivisionCommand(
    Guid CompetitionId,
    Guid CompetitionDivisionId,
    Guid CompetitionTeamId) : IRequest<Result<HockeyCompetitionDto>>;
