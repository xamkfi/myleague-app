using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball;

/// <summary>
/// Request model for creating a floorball tournament
/// </summary>
public class CreateFloorballTournamentRequest
{
    /// <summary>
    /// Name of the tournament
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the tournament (ISO 8601 format)
    /// </summary>
    [Required]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// End date of the tournament (ISO 8601 format)
    /// </summary>
    [Required]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Location or venue of the tournament
    /// </summary>
    [StringLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// HTML description of the tournament
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Number of regular periods. Default: 2.
    /// </summary>
    public int NumberOfPeriods { get; set; } = 2;

    /// <summary>
    /// Duration in minutes per regular period. Default: 15.
    /// </summary>
    public int PeriodDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether overtime is allowed when the match is tied. Default: true.
    /// </summary>
    public bool AllowOvertime { get; set; } = true;

    /// <summary>
    /// Duration in minutes for the overtime period. Default: 5.
    /// </summary>
    public int OvertimeDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Whether shootout is allowed after overtime. Default: true.
    /// </summary>
    public bool AllowShootout { get; set; } = true;

    /// <summary>
    /// Playoff format (None, SingleElimination, FinalGroup). Default: None.
    /// </summary>
    [StringLength(50)]
    public string PlayoffFormat { get; set; } = "None";

    /// <summary>
    /// Number of teams advancing from each group to playoffs. Default: 1.
    /// </summary>
    public int GroupStageAdvancingCount { get; set; } = 1;
}

/// <summary>
/// Request model for updating a floorball tournament
/// </summary>
public class UpdateFloorballTournamentRequest
{
    /// <summary>
    /// Name of the tournament
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the tournament (ISO 8601 format)
    /// </summary>
    [Required]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// End date of the tournament (ISO 8601 format)
    /// </summary>
    [Required]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Location or venue of the tournament
    /// </summary>
    [StringLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// HTML description of the tournament
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Number of regular periods. Default: 2.
    /// </summary>
    public int NumberOfPeriods { get; set; } = 2;

    /// <summary>
    /// Duration in minutes per regular period. Default: 15.
    /// </summary>
    public int PeriodDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether overtime is allowed when the match is tied. Default: true.
    /// </summary>
    public bool AllowOvertime { get; set; } = true;

    /// <summary>
    /// Duration in minutes for the overtime period. Default: 5.
    /// </summary>
    public int OvertimeDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Whether shootout is allowed after overtime. Default: true.
    /// </summary>
    public bool AllowShootout { get; set; } = true;

    /// <summary>
    /// Playoff format (None, SingleElimination, FinalGroup). Default: None.
    /// </summary>
    [StringLength(50)]
    public string PlayoffFormat { get; set; } = "None";

    /// <summary>
    /// Number of teams advancing from each group to playoffs. Default: 1.
    /// </summary>
    public int GroupStageAdvancingCount { get; set; } = 1;
}

/// <summary>
/// Request model for adding a group to a floorball tournament
/// </summary>
public class AddGroupToTournamentRequest
{
    /// <summary>
    /// Name of the group (e.g. A-lohko)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Phase of the group (GroupStage or Playoff). Default: GroupStage.
    /// </summary>
    [StringLength(50)]
    public string Phase { get; set; } = "GroupStage";

    /// <summary>
    /// Display sort order for the group
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Request model for changing a floorball tournament's status
/// </summary>
public class ChangeFloorballTournamentStatusRequest
{
    /// <summary>
    /// The action to perform (e.g. activate, start, complete, cancel)
    /// </summary>
    [Required]
    public string Action { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a match within a floorball tournament
/// </summary>
public class CreateTournamentMatchRequest
{
    /// <summary>
    /// ID of the home team
    /// </summary>
    [Required]
    public Guid HomeTeamId { get; set; }

    /// <summary>
    /// ID of the away team
    /// </summary>
    [Required]
    public Guid AwayTeamId { get; set; }

    /// <summary>
    /// Scheduled date and time (ISO 8601 format)
    /// </summary>
    [Required]
    public string ScheduledDateTime { get; set; } = string.Empty;

    /// <summary>
    /// Venue or location for the match
    /// </summary>
    public string? Venue { get; set; }

    /// <summary>
    /// Optional tournament group ID to associate the match with
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// Optional tournament round (e.g. QuarterFinal, SemiFinal, Final)
    /// </summary>
    public string? TournamentRound { get; set; }

    /// <summary>
    /// Optional referee ID
    /// </summary>
    public Guid? RefereeId { get; set; }
}
