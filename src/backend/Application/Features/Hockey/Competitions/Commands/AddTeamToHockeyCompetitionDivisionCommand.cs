using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to place a competition team into a competition division.
/// </summary>
public record AddTeamToHockeyCompetitionDivisionCommand(
    Guid CompetitionId,
    Guid CompetitionDivisionId,
    Guid CompetitionTeamId,
    int? Seed = null) : IRequest<Result<HockeyCompetitionDto>>;
