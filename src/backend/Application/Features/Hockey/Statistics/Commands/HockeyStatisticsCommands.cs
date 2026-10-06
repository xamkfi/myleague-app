using Application.Common;
using Domain.Enums.Hockey.Statistics;
using MediatR;

namespace Application.Features.Hockey.Statistics.Commands;

/// <summary>
/// Recalculates and replaces match-level statistics for a hockey match.
/// </summary>
public record RecalculateHockeyMatchStatisticsCommand(Guid MatchId) : IRequest<Result>;

/// <summary>
/// Recalculates and replaces competition aggregate statistics for a scope.
/// </summary>
public record RecalculateHockeyCompetitionStatisticsCommand(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result>;

/// <summary>
/// One aggregate scope inside a competition.
/// </summary>
public record HockeyStatisticsScopeTarget(
    HockeyStatisticsScope Scope,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null);

/// <summary>
/// Recalculates several aggregate scopes of one competition from a single match and roster load.
/// </summary>
public record RecalculateHockeyCompetitionScopesCommand(
    Guid CompetitionId,
    IReadOnlyList<HockeyStatisticsScopeTarget> Scopes) : IRequest<Result>;

/// <summary>
/// Deletes competition aggregate statistics for a competition (optionally scoped) without rebuilding.
/// </summary>
public record ResetHockeyCompetitionStatisticsCommand(
    Guid CompetitionId,
    HockeyStatisticsScope? Scope = null,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result>;
