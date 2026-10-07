using System.ComponentModel.DataAnnotations;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Enums.Hockey.Teams;

namespace WebAPI.Models.Hockey;

/// <summary>
/// Request body for recording a hockey goal.
/// </summary>
public class RecordHockeyGoalRequest
{
    /// <summary>
    /// Scoring Match Team Id.
    /// </summary>
    [Required]
    public Guid ScoringMatchTeamId { get; set; }

    /// <summary>
    /// Scorer Active Player Id.
    /// </summary>
    [Required]
    public Guid ScorerActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Goal Strength.
    /// </summary>
    [Required]
    public HockeyGoalStrength GoalStrength { get; set; }

    /// <summary>
    /// Primary Assist Active Player Id.
    /// </summary>
    public Guid? PrimaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Secondary Assist Active Player Id.
    /// </summary>
    public Guid? SecondaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Was Empty Net.
    /// </summary>
    public bool WasEmptyNet { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for correcting a hockey goal during live match operations.
/// </summary>
public class UpdateHockeyGoalRequest
{
    /// <summary>
    /// Scoring Match Team Id.
    /// </summary>
    [Required]
    public Guid ScoringMatchTeamId { get; set; }

    /// <summary>
    /// Scorer Active Player Id.
    /// </summary>
    [Required]
    public Guid ScorerActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Goal Strength.
    /// </summary>
    [Required]
    public HockeyGoalStrength GoalStrength { get; set; }

    /// <summary>
    /// Primary Assist Active Player Id.
    /// </summary>
    public Guid? PrimaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Secondary Assist Active Player Id.
    /// </summary>
    public Guid? SecondaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Was Empty Net.
    /// </summary>
    public bool WasEmptyNet { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey penalty.
/// </summary>
public class RecordHockeyPenaltyRequest
{
    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Severity.
    /// </summary>
    [Required]
    public HockeyPenaltySeverity Severity { get; set; }

    /// <summary>
    /// Offence.
    /// </summary>
    [Required]
    public HockeyPenaltyOffence Offence { get; set; }

    /// <summary>
    /// Penalty Minutes.
    /// </summary>
    [Required]
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Penalized Active Player Id.
    /// </summary>
    public Guid? PenalizedActivePlayerId { get; set; }
    /// <summary>
    /// Served By Active Player Id.
    /// </summary>
    public Guid? ServedByActivePlayerId { get; set; }
    /// <summary>
    /// Is Bench Penalty.
    /// </summary>
    public bool IsBenchPenalty { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for correcting a hockey penalty during live match operations.
/// </summary>
public class UpdateHockeyPenaltyRequest
{
    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Severity.
    /// </summary>
    [Required]
    public HockeyPenaltySeverity Severity { get; set; }

    /// <summary>
    /// Offence.
    /// </summary>
    [Required]
    public HockeyPenaltyOffence Offence { get; set; }

    /// <summary>
    /// Penalty Minutes.
    /// </summary>
    [Required]
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Penalized Active Player Id.
    /// </summary>
    public Guid? PenalizedActivePlayerId { get; set; }
    /// <summary>
    /// Served By Active Player Id.
    /// </summary>
    public Guid? ServedByActivePlayerId { get; set; }
    /// <summary>
    /// Is Bench Penalty.
    /// </summary>
    public bool IsBenchPenalty { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey shot.
/// </summary>
public class RecordHockeyShotRequest
{
    /// <summary>
    /// Shooting Match Team Id.
    /// </summary>
    [Required]
    public Guid ShootingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Result.
    /// </summary>
    [Required]
    public HockeyShotResult ShotResult { get; set; }

    /// <summary>
    /// Counts As Shot On Goal.
    /// </summary>
    public bool CountsAsShotOnGoal { get; set; } = true;
    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    public Guid? ShooterActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Number of identical shots to record (1-99). Values above 1 require a saved shot and a goalie.
    /// </summary>
    [Range(1, 99)]
    public int Count { get; set; } = 1;
}

/// <summary>
/// Request body for correcting a hockey shot during live match operations.
/// </summary>
public class UpdateHockeyShotRequest
{
    /// <summary>
    /// Shooting Match Team Id.
    /// </summary>
    [Required]
    public Guid ShootingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Result.
    /// </summary>
    [Required]
    public HockeyShotResult ShotResult { get; set; }

    /// <summary>
    /// Counts As Shot On Goal.
    /// </summary>
    public bool CountsAsShotOnGoal { get; set; } = true;
    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    public Guid? ShooterActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey video review.
/// </summary>
public class RecordHockeyVideoReviewRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Review Type.
    /// </summary>
    [Required]
    public HockeyVideoReviewType ReviewType { get; set; }

    /// <summary>
    /// Original Decision.
    /// </summary>
    [Required]
    public HockeyReviewDecision OriginalDecision { get; set; }

    /// <summary>
    /// Final Decision.
    /// </summary>
    [Required]
    public HockeyReviewDecision FinalDecision { get; set; }

    /// <summary>
    /// Is Coach Challenge.
    /// </summary>
    public bool IsCoachChallenge { get; set; }
    /// <summary>
    /// Was Successful.
    /// </summary>
    public bool WasSuccessful { get; set; }
    /// <summary>
    /// Requested By Match Team Id.
    /// </summary>
    public Guid? RequestedByMatchTeamId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a period event.
/// </summary>
public class RecordHockeyPeriodEventRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Action.
    /// </summary>
    [Required]
    public HockeyPeriodAction Action { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a faceoff.
/// </summary>
public class RecordHockeyFaceoffRequest
{
    /// <summary>
    /// Winning Match Team Id.
    /// </summary>
    [Required]
    public Guid WinningMatchTeamId { get; set; }

    /// <summary>
    /// Losing Match Team Id.
    /// </summary>
    [Required]
    public Guid LosingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Zone.
    /// </summary>
    [Required]
    public HockeyFaceoffZone Zone { get; set; }

    /// <summary>
    /// Spot.
    /// </summary>
    [Required]
    public HockeyFaceoffSpot Spot { get; set; }

    /// <summary>
    /// Winning Active Player Id.
    /// </summary>
    public Guid? WinningActivePlayerId { get; set; }
    /// <summary>
    /// Losing Active Player Id.
    /// </summary>
    public Guid? LosingActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a stoppage.
/// </summary>
public class RecordHockeyStoppageRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Reason.
    /// </summary>
    [Required]
    public HockeyStoppageReason Reason { get; set; }

    /// <summary>
    /// Responsible Match Team Id.
    /// </summary>
    public Guid? ResponsibleMatchTeamId { get; set; }
    /// <summary>
    /// Responsible Active Player Id.
    /// </summary>
    public Guid? ResponsibleActivePlayerId { get; set; }
    /// <summary>
    /// Next Faceoff Zone.
    /// </summary>
    public HockeyFaceoffZone? NextFaceoffZone { get; set; }
    /// <summary>
    /// Next Faceoff Spot.
    /// </summary>
    public HockeyFaceoffSpot? NextFaceoffSpot { get; set; }
    /// <summary>
    /// Rule Reference.
    /// </summary>
    public string? RuleReference { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a timeout.
/// </summary>
public class RecordHockeyTimeoutRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a goalie change.
/// </summary>
public class RecordHockeyGoalieChangeRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Outgoing Goalie Active Player Id.
    /// </summary>
    public Guid? OutgoingGoalieActivePlayerId { get; set; }
    /// <summary>
    /// Incoming Goalie Active Player Id.
    /// </summary>
    public Guid? IncomingGoalieActivePlayerId { get; set; }
    /// <summary>
    /// Reason.
    /// </summary>
    public string? Reason { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a shootout attempt.
/// </summary>
public class RecordHockeyShootoutAttemptRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    [Required]
    public Guid ShooterActivePlayerId { get; set; }

    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    [Required]
    public Guid GoalieActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Order.
    /// </summary>
    [Required]
    public int ShotOrder { get; set; }

    /// <summary>
    /// Result.
    /// </summary>
    [Required]
    public HockeyShootoutAttemptResult Result { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a failed coach-challenge penalty.
/// </summary>
public class RecordHockeyFailedCoachChallengePenaltyRequest
{
    /// <summary>
    /// Video Review Id.
    /// </summary>
    [Required]
    public Guid VideoReviewId { get; set; }

    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// Max Challenges Per Team.
    /// </summary>
    public int MaxChallengesPerTeam { get; set; } = 1;
    /// <summary>
    /// Lose Challenge After Failed.
    /// </summary>
    public bool LoseChallengeAfterFailed { get; set; } = true;
    /// <summary>
    /// Penalty For Failed Challenge.
    /// </summary>
    public bool PenaltyForFailedChallenge { get; set; } = true;
    /// <summary>
    /// Failed Challenge Penalty Minutes.
    /// </summary>
    public int FailedChallengePenaltyMinutes { get; set; } = 2;
    /// <summary>
    /// Failed Challenge Penalty Offence.
    /// </summary>
    public HockeyPenaltyOffence FailedChallengePenaltyOffence { get; set; } = HockeyPenaltyOffence.DelayOfGame;
    /// <summary>
    /// Failed Challenge Penalty Severity.
    /// </summary>
    public HockeyPenaltySeverity FailedChallengePenaltySeverity { get; set; } = HockeyPenaltySeverity.Minor;
    /// <summary>
    /// Allow Challenge In Overtime.
    /// </summary>
    public bool AllowChallengeInOvertime { get; set; } = true;
    /// <summary>
    /// Allow Challenge In Shootout.
    /// </summary>
    public bool AllowChallengeInShootout { get; set; }
}

/// <summary>
/// Batch import of historical hockey match events. Intended for the JoomLeague
/// importer and admin backfill, not live scorekeeping.
/// </summary>
public class ImportHockeyMatchEventsRequest
{
    /// <summary>
    /// Goals and penalties to record, in clock order.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public List<ImportHockeyMatchEventRequest> Events { get; set; } = [];
}

/// <summary>
/// One event in an <see cref="ImportHockeyMatchEventsRequest"/> batch.
/// <c>EventType</c> is <c>Goal</c> or <c>Penalty</c>. Player ids are dressed
/// match-active-player ids, matching the live goal and penalty endpoints.
/// </summary>
public class ImportHockeyMatchEventRequest
{
    /// <summary>
    /// Event kind: <c>Goal</c> or <c>Penalty</c>.
    /// </summary>
    [Required]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Match-team id of the scoring or penalized side.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Dressed player who scored or received the penalty.
    /// </summary>
    public Guid? ActivePlayerId { get; set; }

    /// <summary>
    /// Primary assister for a goal, when recorded.
    /// </summary>
    public Guid? PrimaryAssistActivePlayerId { get; set; }

    /// <summary>
    /// Secondary assister for a goal, when recorded.
    /// </summary>
    public Guid? SecondaryAssistActivePlayerId { get; set; }

    /// <summary>
    /// Opposing goalie for a goal, when known.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }

    /// <summary>
    /// Period in which the event occurred.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Elapsed match clock time, in seconds.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Manpower situation for a goal. Defaults to even strength when omitted.
    /// </summary>
    public HockeyGoalStrength? GoalStrength { get; set; }

    /// <summary>
    /// Whether the goal was scored on an empty net.
    /// </summary>
    public bool WasEmptyNet { get; set; }

    /// <summary>
    /// Optional free-text description of the event.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Penalty severity. Used when <see cref="EventType"/> is <c>Penalty</c>.
    /// </summary>
    public HockeyPenaltySeverity? Severity { get; set; }

    /// <summary>
    /// Penalty offence. Used when <see cref="EventType"/> is <c>Penalty</c>.
    /// </summary>
    public HockeyPenaltyOffence? Offence { get; set; }

    /// <summary>
    /// Penalty length in minutes. Used when <see cref="EventType"/> is <c>Penalty</c>.
    /// </summary>
    [Range(0, 20)]
    public int? PenaltyMinutes { get; set; }

    /// <summary>
    /// Player serving a bench or coincidental penalty, when different from the offender.
    /// </summary>
    public Guid? ServedByActivePlayerId { get; set; }

    /// <summary>
    /// Whether this is a bench penalty (no individual offender).
    /// </summary>
    public bool IsBenchPenalty { get; set; }
}
