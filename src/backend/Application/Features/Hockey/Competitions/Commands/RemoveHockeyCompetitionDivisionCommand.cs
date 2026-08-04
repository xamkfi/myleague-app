using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to soft-remove a competition division.
/// </summary>
public record RemoveHockeyCompetitionDivisionCommand(
    Guid CompetitionId,
    Guid CompetitionDivisionId) : IRequest<Result<HockeyCompetitionDto>>;
