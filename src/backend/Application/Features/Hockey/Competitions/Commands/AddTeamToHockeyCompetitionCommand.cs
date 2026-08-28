using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command for adding a hockey team to a competition (season or tournament).
/// </summary>
/// <param name="CompetitionId">Competition to join</param>
/// <param name="TeamId">Hockey team to add</param>
/// <param name="Seed">Optional seeding value</param>
public record AddTeamToHockeyCompetitionCommand(
    Guid CompetitionId,
    Guid TeamId,
    int? Seed = null) : IRequest<Result<HockeyCompetitionTeamDto>>;
