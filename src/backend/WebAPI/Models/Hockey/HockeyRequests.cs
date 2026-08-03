using System.ComponentModel.DataAnnotations;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;

namespace WebAPI.Models.Hockey;

/// <summary>
/// Request body for creating a hockey season.
/// </summary>
public class CreateHockeySeasonRequest
{
    /// <summary>
    /// Name of the season.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Season start date.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Season end date.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional short season code.
    /// </summary>
    [StringLength(50)]
    public string? SeasonCode { get; set; }
}

/// <summary>
/// Request body for creating a hockey tournament.
/// </summary>
public class CreateHockeyTournamentRequest
{
    /// <summary>
    /// Name of the tournament.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tournament start date.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Tournament end date.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional primary venue.
    /// </summary>
    [StringLength(200)]
    public string? Venue { get; set; }

    /// <summary>
    /// Optional HTML description.
    /// </summary>
    public string? ContentHtml { get; set; }
}

/// <summary>
/// Request body for creating a hockey team.
/// </summary>
public class CreateHockeyTeamRequest
{
    /// <summary>
    /// Team name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Owning club id.
    /// </summary>
    [Required]
    public Guid ClubId { get; set; }

    /// <summary>
    /// Team category (adult, youth, or women).
    /// </summary>
    [Required]
    public TeamCategory TeamCategory { get; set; }

    /// <summary>
    /// Optional division id.
    /// </summary>
    public Guid? DivisionId { get; set; }

    /// <summary>
    /// Optional home arena.
    /// </summary>
    [StringLength(200)]
    public string? HomeArena { get; set; }

    /// <summary>
    /// Optional primary jersey color.
    /// </summary>
    [StringLength(50)]
    public string? PrimaryJerseyColor { get; set; }

    /// <summary>
    /// Optional secondary jersey color.
    /// </summary>
    [StringLength(50)]
    public string? SecondaryJerseyColor { get; set; }

    /// <summary>
    /// Optional short name.
    /// </summary>
    [StringLength(50)]
    public string? ShortName { get; set; }
}

/// <summary>
/// Request body for adding a team to a hockey competition.
/// </summary>
public class AddTeamToHockeyCompetitionRequest
{
    /// <summary>
    /// Hockey team id to add.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Optional seeding value.
    /// </summary>
    public int? Seed { get; set; }
}

/// <summary>
/// Request body for creating a hockey tournament group (lohko).
/// </summary>
public class CreateHockeyTournamentGroupRequest
{
    /// <summary>
    /// Group display name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request body for adding a competition team to a hockey tournament group.
/// </summary>
public class AddTeamToHockeyTournamentGroupRequest
{
    /// <summary>
    /// Competition-team membership id (not raw HockeyTeam id).
    /// </summary>
    [Required]
    public Guid CompetitionTeamId { get; set; }

    /// <summary>
    /// Optional seed within the group.
    /// </summary>
    public int? Seed { get; set; }
}

/// <summary>
/// Request body for updating a hockey tournament.
/// </summary>
public class UpdateHockeyTournamentRequest
{
    /// <summary>Tournament name.</summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Start date.</summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>End date.</summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>Optional venue.</summary>
    [StringLength(200)]
    public string? Venue { get; set; }

    /// <summary>Optional HTML content.</summary>
    public string? ContentHtml { get; set; }
}

/// <summary>
/// Request body for updating hockey tournament rules.
/// </summary>
public class UpdateHockeyTournamentRulesRequest
{
    /// <summary>Tournament format.</summary>
    [Required]
    public HockeyTournamentFormat Format { get; set; }

    /// <summary>Whether group stage is enabled.</summary>
    public bool HasGroupStage { get; set; }

    /// <summary>Whether playoffs are enabled.</summary>
    public bool HasPlayoffs { get; set; }

    /// <summary>Whether bronze game is enabled.</summary>
    public bool HasBronzeGame { get; set; }

    /// <summary>Whether placement games are enabled.</summary>
    public bool HasPlacementGames { get; set; }

    /// <summary>Teams advancing per group when playoffs are enabled.</summary>
    public int TeamsAdvancingPerGroup { get; set; }
}

/// <summary>
/// Request body for setting the tournament champion.
/// </summary>
public class SetHockeyTournamentChampionRequest
{
    /// <summary>Champion competition-team id.</summary>
    [Required]
    public Guid ChampionCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for creating a playoff series.
/// </summary>
public class CreateHockeyPlayoffSeriesRequest
{
    /// <summary>Playoff round.</summary>
    [Required]
    public HockeyPlayoffRound Round { get; set; }

    /// <summary>Order within the round.</summary>
    [Required]
    public int SeriesOrder { get; set; }

    /// <summary>Best-of value (minimum 1).</summary>
    [Required]
    public int BestOf { get; set; }

    /// <summary>Optional home competition-team id.</summary>
    public Guid? HomeCompetitionTeamId { get; set; }

    /// <summary>Optional away competition-team id.</summary>
    public Guid? AwayCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for assigning teams to a playoff series.
/// </summary>
public class AssignHockeyPlayoffSeriesTeamsRequest
{
    /// <summary>Home competition-team id.</summary>
    [Required]
    public Guid HomeCompetitionTeamId { get; set; }

    /// <summary>Away competition-team id.</summary>
    [Required]
    public Guid AwayCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for replacing the tournament playoff schedule.
/// </summary>
public class SetHockeyTournamentPlayoffScheduleRequest
{
    /// <summary>Schedule slots.</summary>
    [Required]
    public List<HockeyPlayoffScheduleSlotDto> Slots { get; set; } = new();
}
