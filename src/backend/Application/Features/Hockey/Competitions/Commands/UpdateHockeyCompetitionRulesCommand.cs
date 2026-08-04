using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Replaces competition rules, including nested match/standing/roster/video/contact sections.
/// </summary>
public record UpdateHockeyCompetitionRulesCommand(
    Guid CompetitionId,
    HockeyCompetitionRulesInputDto Rules) : IRequest<Result<HockeyCompetitionDto>>;
