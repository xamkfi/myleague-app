using System.ComponentModel.DataAnnotations;
using Domain.Enums.Football;

namespace WebAPI.Models.Football;

/// <summary>
/// One pre-defined playoff bracket slot. Sent by the tournament import flow so the
/// schedule view can show "TBD vs TBD" placeholders before the bracket is generated.
/// </summary>
public class PlayoffScheduleSlotRequest
{
    /// <summary>
    /// Playoff round this slot belongs to
    /// </summary>
    [Required]
    public FootballPlayoffRound Round { get; set; }

    /// <summary>
    /// Display order of the slot within the round
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// Scheduled kickoff time for the slot
    /// </summary>
    [Required]
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Optional venue for the slot
    /// </summary>
    public string? Venue { get; set; }
}

/// <summary>
/// Request model for creating a football tournament
/// </summary>
public class CreateFootballTournamentRequest
{
    /// <summary>
    /// Tournament name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tournament start date
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Tournament end date
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional venue
    /// </summary>
    public string? Venue { get; set; }

    /// <summary>
    /// Optional HTML content describing the tournament
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Number of halves in group-stage matches
    /// </summary>
    public int GroupStageNumberOfHalves { get; set; } = 2;

    /// <summary>
    /// Duration of each group-stage half in minutes
    /// </summary>
    public int GroupStageHalfDurationMinutes { get; set; } = 45;

    /// <summary>
    /// Number of players on the field in group-stage matches
    /// </summary>
    public int GroupStagePlayersOnField { get; set; } = 11;

    /// <summary>
    /// Whether a goalkeeper is required in group-stage matches
    /// </summary>
    public bool GroupStageRequireGoalkeeper { get; set; } = true;

    /// <summary>
    /// Maximum substitutions in group-stage matches
    /// </summary>
    public int GroupStageMaxSubstitutions { get; set; } = 0;

    /// <summary>
    /// Whether officials must be assigned before group-stage kickoff
    /// </summary>
    public bool GroupStageRequireOfficialsToStart { get; set; } = false;

    /// <summary>
    /// Whether extra time is allowed in group-stage matches
    /// </summary>
    public bool GroupStageAllowExtraTime { get; set; } = false;

    /// <summary>
    /// Number of extra-time halves in group-stage matches
    /// </summary>
    public int GroupStageExtraTimeHalfCount { get; set; } = 2;

    /// <summary>
    /// Duration of each group-stage extra-time half in minutes
    /// </summary>
    public int GroupStageExtraTimeHalfDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether a penalty shootout is allowed in group-stage matches
    /// </summary>
    public bool GroupStageAllowPenaltyShootout { get; set; } = false;

    /// <summary>
    /// Number of halves in playoff matches
    /// </summary>
    public int PlayoffNumberOfHalves { get; set; } = 2;

    /// <summary>
    /// Duration of each playoff half in minutes
    /// </summary>
    public int PlayoffHalfDurationMinutes { get; set; } = 45;

    /// <summary>
    /// Number of players on the field in playoff matches
    /// </summary>
    public int PlayoffPlayersOnField { get; set; } = 11;

    /// <summary>
    /// Whether a goalkeeper is required in playoff matches
    /// </summary>
    public bool PlayoffRequireGoalkeeper { get; set; } = true;

    /// <summary>
    /// Maximum substitutions in playoff matches
    /// </summary>
    public int PlayoffMaxSubstitutions { get; set; } = 0;

    /// <summary>
    /// Whether officials must be assigned before playoff kickoff
    /// </summary>
    public bool PlayoffRequireOfficialsToStart { get; set; } = false;

    /// <summary>
    /// Whether extra time is allowed in playoff matches
    /// </summary>
    public bool PlayoffAllowExtraTime { get; set; } = true;

    /// <summary>
    /// Number of extra-time halves in playoff matches
    /// </summary>
    public int PlayoffExtraTimeHalfCount { get; set; } = 2;

    /// <summary>
    /// Duration of each playoff extra-time half in minutes
    /// </summary>
    public int PlayoffExtraTimeHalfDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether a penalty shootout is allowed in playoff matches
    /// </summary>
    public bool PlayoffAllowPenaltyShootout { get; set; } = true;

    /// <summary>
    /// Number of teams advancing from each group
    /// </summary>
    public int TeamsAdvancingPerGroup { get; set; } = 2;

    /// <summary>
    /// Whether the tournament includes a playoff stage
    /// </summary>
    public bool HasPlayoffStage { get; set; } = true;

    /// <summary>
    /// Whether the tournament includes a third-place match
    /// </summary>
    public bool HasThirdPlaceMatch { get; set; } = false;

    /// <summary>
    /// Optional pre-defined playoff schedule slots
    /// </summary>
    public List<PlayoffScheduleSlotRequest>? PlayoffSchedule { get; set; }

    /// <summary>
    /// Audience / age-group category for the tournament
    /// </summary>
    public Domain.Enums.Common.TeamCategory? TeamCategory { get; set; }
}

/// <summary>
/// Request model for updating a football tournament
/// </summary>
public class UpdateFootballTournamentRequest : CreateFootballTournamentRequest { }

/// <summary>
/// Request model for adding a group to a tournament
/// </summary>
public class AddGroupToTournamentRequest
{
    /// <summary>
    /// Name of the group to add
    /// </summary>
    [Required]
    [StringLength(50)]
    public string GroupName { get; set; } = string.Empty;
}

/// <summary>
/// Request model for adding a team to a tournament group
/// </summary>
public class AddTeamToTournamentGroupRequest
{
    /// <summary>
    /// Identifier of the team to add
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }
}
