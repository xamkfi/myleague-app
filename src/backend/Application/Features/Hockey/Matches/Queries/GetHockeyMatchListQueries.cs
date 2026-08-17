using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Queries;

/// <summary>
/// Gets hockey matches for a competition (season or tournament).
/// </summary>
public record GetHockeyMatchesByCompetitionQuery(Guid CompetitionId)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Gets hockey matches involving a career team (home or away).
/// </summary>
public record GetHockeyMatchesByTeamQuery(Guid TeamId)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;
