using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

public record AddTeamToHockeyCompetitionCommand(
    Guid CompetitionId,
    Guid TeamId,
    int? Seed = null) : IRequest<Result<HockeyCompetitionTeamDto>>;
