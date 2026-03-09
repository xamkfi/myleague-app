using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Matches.DTOs
{
    /// <summary>
    /// Data Transfer Object for period score
    /// </summary>
    /// <param name="HomeScore">The home team's score for this period</param>
    /// <param name="AwayScore">The away team's score for this period</param>
    /// <param name="IsCompleted">Whether the period has been completed</param>
    public record PeriodScoreDto(int HomeScore, int AwayScore, bool IsCompleted);

    /// <summary>
    /// Data Transfer Object for FloorballMatch entity.
    /// Supports both season and tournament matches via nullable context fields.
    /// </summary>
    public record FloorballMatchDto(
        Guid Id,
        Guid? SeasonId,
        string? SeasonName,
        Guid? TournamentId,
        string? TournamentName,
        Guid? TournamentGroupId,
        string? TournamentRound,
        Guid HomeTeamId,
        string HomeTeamName,
        Uri? HomeTeamLogo,
        Guid AwayTeamId,
        string AwayTeamName,
        Uri? AwayTeamLogo,
        DateTime ScheduledDateTime,
        string? Venue,
        FloorballMatchStatus Status,
        int HomeScore,
        int AwayScore,
        bool WentToOvertime,
        bool WentToShootout,
        Guid? HomeActiveGoalieId,
        Guid? AwayActiveGoalieId,
        IReadOnlyDictionary<int, PeriodScoreDto> PeriodScores,
        IReadOnlyCollection<Guid> Officials,
        IReadOnlyCollection<FloorballGoalEventDto> GoalEvents,
        IReadOnlyCollection<FloorballPenaltyEventDto> PenaltyEvents,
        IReadOnlyCollection<FloorballSaveEventDto> SaveEvents,
        FloorballMatchRulesDto MatchRules);
}
