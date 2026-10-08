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
/// Request body for creating a hockey match.
/// </summary>
public class CreateHockeyMatchRequest
{
    /// <summary>
    /// Scheduled Start Time.
    /// </summary>
    [Required]
    public DateTime ScheduledStartTime { get; set; }

    /// <summary>
    /// Match Type.
    /// </summary>
    [Required]
    public HockeyMatchType MatchType { get; set; }

    /// <summary>
    /// Competition Id.
    /// </summary>
    public Guid? CompetitionId { get; set; }
    /// <summary>
    /// Competition Division Id.
    /// </summary>
    public Guid? CompetitionDivisionId { get; set; }
    /// <summary>
    /// Tournament Group Id.
    /// </summary>
    public Guid? TournamentGroupId { get; set; }
    /// <summary>
    /// Playoff Series Id.
    /// </summary>
    public Guid? PlayoffSeriesId { get; set; }

    /// <summary>
    /// Playoff round for bracket advancement.
    /// </summary>
    public HockeyPlayoffRound? PlayoffRound { get; set; }

    /// <summary>
    /// Order of this match within the playoff round.
    /// </summary>
    public int? PlayoffMatchOrder { get; set; }

    /// <summary>
    /// Match the winner advances into.
    /// </summary>
    public Guid? NextMatchId { get; set; }

    /// <summary>
    /// Home or away slot on the next match.
    /// </summary>
    public HockeyTeamSlot? NextMatchSlot { get; set; }

    /// <summary>
    /// Venue.
    /// </summary>
    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for assigning home/away teams to a hockey match.
/// </summary>
public class AddHomeAwayTeamsToHockeyMatchRequest
{
    /// <summary>
    /// Home Team Id.
    /// </summary>
    [Required]
    public Guid HomeTeamId { get; set; }

    /// <summary>
    /// Away Team Id.
    /// </summary>
    [Required]
    public Guid AwayTeamId { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as started.
/// </summary>
public class MarkHockeyMatchStartedRequest
{
    /// <summary>
    /// Actual Start Time.
    /// </summary>
    public DateTime? ActualStartTime { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as finished.
/// </summary>
public class MarkHockeyMatchFinishedRequest
{
    /// <summary>
    /// Actual End Time.
    /// </summary>
    public DateTime? ActualEndTime { get; set; }
    /// <summary>
    /// Result Type.
    /// </summary>
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting hockey match status.
/// </summary>
public class SetHockeyMatchStatusRequest
{
    /// <summary>
    /// Status.
    /// </summary>
    [Required]
    public HockeyMatchStatus Status { get; set; }
}

/// <summary>
/// Request body for setting hockey match result type.
/// </summary>
public class SetHockeyMatchResultTypeRequest
{
    /// <summary>
    /// Result Type.
    /// </summary>
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting the current period.
/// </summary>
public class SetHockeyMatchCurrentPeriodRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }
}

/// <summary>
/// Request body for overtime / shootout flags.
/// </summary>
public class SetHockeyMatchBooleanFlagRequest
{
    /// <summary>
    /// Value.
    /// </summary>
    [Required]
    public bool Value { get; set; }
}

/// <summary>
/// Request body for updating match venue.
/// </summary>
public class UpdateHockeyMatchVenueRequest
{
    /// <summary>
    /// Venue.
    /// </summary>
    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for updating scheduled start.
/// </summary>
public class UpdateHockeyMatchScheduledStartRequest
{
    /// <summary>
    /// Scheduled Start Time.
    /// </summary>
    [Required]
    public DateTime ScheduledStartTime { get; set; }
}

/// <summary>
/// Request body for correcting team goals.
/// </summary>
public class SetHockeyMatchTeamGoalsRequest
{
    /// <summary>
    /// Team Slot.
    /// </summary>
    [Required]
    public HockeyTeamSlot TeamSlot { get; set; }

    /// <summary>
    /// Goals.
    /// </summary>
    [Required]
    public int Goals { get; set; }
}

/// <summary>
/// Request body for creating a period score row.
/// </summary>
public class AddHockeyPeriodScoreRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Period Type.
    /// </summary>
    [Required]
    public HockeyPeriodType PeriodType { get; set; }
}
