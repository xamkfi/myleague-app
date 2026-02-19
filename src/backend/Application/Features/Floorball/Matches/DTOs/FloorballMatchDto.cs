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
    /// Data Transfer Object for FloorballMatch entity
    /// </summary>
    /// <param name="Id">The unique identifier of the match</param>
    /// <param name="SeasonId">The ID of the season this match belongs to</param>
    /// <param name="HomeTeamId">The ID of the home team</param>
    /// <param name="HomeTeamName">The name of the home team</param>
    /// <param name="AwayTeamId">The ID of the away team</param>
    /// <param name="AwayTeamName">The name of the away team</param>
    /// <param name="ScheduledDateTime">The scheduled date and time of the match</param>
    /// <param name="Venue">The venue where the match will be played</param>
    /// <param name="Status">The current status of the match</param>
    /// <param name="HomeScore">The home team's score</param>
    /// <param name="AwayScore">The away team's score</param>
    /// <param name="WentToOvertime">Whether the match went to overtime</param>
    /// <param name="WentToShootout">Whether the match went to shootout</param>
    /// <param name="PeriodScores">The scores for each period</param>
    /// <param name="Officials">The IDs of the match officials (referees)</param>
    /// <param name="GoalEvents">The goals scored in the match</param>
    /// <param name="PenaltyEvents">The penalties given in the match</param>
    /// <param name="MatchRules">The match rules configuration snapshot</param>
    public record FloorballMatchDto(
        Guid Id,
        Guid SeasonId,
        string SeasonName,
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
