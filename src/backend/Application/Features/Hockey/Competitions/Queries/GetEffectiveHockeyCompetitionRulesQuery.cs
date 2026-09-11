using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Queries;

/// <summary>
/// Gets effective competition rules (including tournament match-rule overrides).
/// </summary>
public record GetEffectiveHockeyCompetitionRulesQuery(Guid CompetitionId)
    : IRequest<Result<HockeyCompetitionRulesDto>>;
