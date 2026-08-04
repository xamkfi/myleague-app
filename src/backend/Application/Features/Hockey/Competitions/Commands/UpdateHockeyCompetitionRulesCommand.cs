using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using Domain.Enums.Hockey.Competitions;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to replace competition rules (nested match/standing/roster use Domain defaults).
/// </summary>
public record UpdateHockeyCompetitionRulesCommand(
    Guid CompetitionId,
    string Name,
    string? RuleBookVersion,
    HockeyRuleBookSource RuleBookSource) : IRequest<Result<HockeyCompetitionDto>>;
