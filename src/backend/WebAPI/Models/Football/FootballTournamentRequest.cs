using System.ComponentModel.DataAnnotations;
using Domain.Enums.Football;

namespace WebAPI.Models.Football;

/// <summary>
/// One pre-defined playoff bracket slot. Sent by the tournament import flow so the
/// schedule view can show "TBD vs TBD" placeholders before the bracket is generated.
/// </summary>
public class PlayoffScheduleSlotRequest
{
    [Required]
    public FootballPlayoffRound Round { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    public DateTime ScheduledDateTime { get; set; }

    public string? Venue { get; set; }
}

/// <summary>
/// Request model for creating a football tournament
/// </summary>
public class CreateFootballTournamentRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? Venue { get; set; }

    public string? ContentHtml { get; set; }

    public int GroupStageNumberOfHalves { get; set; } = 2;
    public int GroupStageHalfDurationMinutes { get; set; } = 45;
    public int GroupStagePlayersOnField { get; set; } = 11;
    public bool GroupStageRequireGoalkeeper { get; set; } = true;
    public int GroupStageMaxSubstitutions { get; set; } = 0;
    public bool GroupStageRequireOfficialsToStart { get; set; } = false;
    public bool GroupStageAllowExtraTime { get; set; } = false;
    public int GroupStageExtraTimeHalfCount { get; set; } = 2;
    public int GroupStageExtraTimeHalfDurationMinutes { get; set; } = 15;
    public bool GroupStageAllowPenaltyShootout { get; set; } = false;

    public int PlayoffNumberOfHalves { get; set; } = 2;
    public int PlayoffHalfDurationMinutes { get; set; } = 45;
    public int PlayoffPlayersOnField { get; set; } = 11;
    public bool PlayoffRequireGoalkeeper { get; set; } = true;
    public int PlayoffMaxSubstitutions { get; set; } = 0;
    public bool PlayoffRequireOfficialsToStart { get; set; } = false;
    public bool PlayoffAllowExtraTime { get; set; } = true;
    public int PlayoffExtraTimeHalfCount { get; set; } = 2;
    public int PlayoffExtraTimeHalfDurationMinutes { get; set; } = 15;
    public bool PlayoffAllowPenaltyShootout { get; set; } = true;

    public int TeamsAdvancingPerGroup { get; set; } = 2;
    public bool HasPlayoffStage { get; set; } = true;
    public bool HasThirdPlaceMatch { get; set; } = false;

    public List<PlayoffScheduleSlotRequest>? PlayoffSchedule { get; set; }

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
    [Required]
    [StringLength(50)]
    public string GroupName { get; set; } = string.Empty;
}

/// <summary>
/// Request model for adding a team to a tournament group
/// </summary>
public class AddTeamToTournamentGroupRequest
{
    [Required]
    public Guid TeamId { get; set; }
}
