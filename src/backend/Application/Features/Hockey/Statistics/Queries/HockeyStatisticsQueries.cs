using Application.Common;
using Application.Features.Hockey.Statistics.DTOs;
using Domain.Enums.Hockey.Statistics;
using MediatR;

namespace Application.Features.Hockey.Statistics.Queries;

/// <summary>
/// Gets match-level box score statistics.
/// </summary>
public record GetHockeyMatchStatisticsQuery(Guid MatchId) : IRequest<Result<HockeyMatchStatisticsDto>>;

/// <summary>
/// Gets competition-scope standings.
/// </summary>
public record GetHockeyCompetitionStandingsQuery(Guid CompetitionId)
    : IRequest<Result<List<HockeyTeamCompetitionStatisticsDto>>>;

/// <summary>
/// Gets division-scope standings.
/// </summary>
public record GetHockeyDivisionStandingsQuery(Guid CompetitionId, Guid CompetitionDivisionId)
    : IRequest<Result<List<HockeyTeamCompetitionStatisticsDto>>>;

/// <summary>
/// Gets tournament-group standings.
/// </summary>
public record GetHockeyTournamentGroupStandingsQuery(Guid CompetitionId, Guid TournamentGroupId)
    : IRequest<Result<List<HockeyTeamCompetitionStatisticsDto>>>;

/// <summary>
/// Gets playoff series statistics (teams, players, goalies).
/// </summary>
public record GetHockeyPlayoffSeriesStatisticsQuery(Guid CompetitionId, Guid PlayoffSeriesId)
    : IRequest<Result<HockeyPlayoffSeriesStatisticsDto>>;

/// <summary>
/// Gets one team's competition statistics at a scope.
/// </summary>
public record GetHockeyTeamCompetitionStatisticsQuery(
    Guid CompetitionId,
    Guid TeamId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result<HockeyTeamCompetitionStatisticsDto>>;

/// <summary>
/// Gets player competition statistics (single player when PlayerId+TeamId set, otherwise list).
/// </summary>
public record GetHockeyPlayerCompetitionStatisticsQuery(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    Guid? PlayerId = null,
    Guid? TeamId = null,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result<List<HockeyPlayerCompetitionStatisticsDto>>>;

/// <summary>
/// Gets goalie competition statistics (single goalie when PlayerId+TeamId set, otherwise list).
/// </summary>
public record GetHockeyGoalieCompetitionStatisticsQuery(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    Guid? PlayerId = null,
    Guid? TeamId = null,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result<List<HockeyGoalieCompetitionStatisticsDto>>>;

/// <summary>
/// Gets top scorers at a scope.
/// </summary>
public record GetHockeyTopScorersQuery(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    int TopN = 10,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result<List<HockeyTopScorerDto>>>;

/// <summary>
/// Gets top goalies at a scope.
/// </summary>
public record GetHockeyTopGoaliesQuery(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    int TopN = 10,
    int MinimumGamesPlayed = 1,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null) : IRequest<Result<List<HockeyTopGoalieDto>>>;

/// <summary>
/// Gets a competition statistics summary dashboard.
/// </summary>
public record GetHockeyCompetitionStatisticsSummaryQuery(
    Guid CompetitionId,
    HockeyStatisticsScope Scope = HockeyStatisticsScope.Competition,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null,
    int TopN = 5) : IRequest<Result<HockeyCompetitionStatisticsSummaryDto>>;
