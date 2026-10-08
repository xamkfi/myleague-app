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
/// Request body for creating a hockey team.
/// </summary>
public class CreateHockeyTeamRequest
{
    /// <summary>
    /// Team name.
    /// </summary>
    [Required]
    [StringLength(100)]
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
    [StringLength(4)]
    public string? ShortName { get; set; }
}

/// <summary>
/// Request body for updating a hockey team.
/// </summary>
public class UpdateHockeyTeamRequest
{
    /// <summary>Team name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional short name.</summary>
    [StringLength(4)]
    public string? ShortName { get; set; }

    /// <summary>Team category.</summary>
    [Required]
    public TeamCategory TeamCategory { get; set; }

    /// <summary>Optional division id.</summary>
    public Guid? DivisionId { get; set; }

    /// <summary>Optional home arena.</summary>
    [StringLength(200)]
    public string? HomeArena { get; set; }

    /// <summary>Optional primary jersey color.</summary>
    [StringLength(50)]
    public string? PrimaryJerseyColor { get; set; }

    /// <summary>Optional secondary jersey color.</summary>
    [StringLength(50)]
    public string? SecondaryJerseyColor { get; set; }
}

/// <summary>
/// Request body for setting team active status.
/// </summary>
public class SetHockeyTeamActiveStatusRequest
{
    /// <summary>Whether the team is active.</summary>
    [Required]
    public bool IsActive { get; set; }
}

/// <summary>
/// Request body for updating a team logo URL.
/// </summary>
public class UpdateHockeyTeamLogoRequest
{
    /// <summary>Absolute logo URL, or null to clear.</summary>
    public string? LogoUrl { get; set; }
}

/// <summary>
/// Request body for adding a player to a hockey team roster.
/// </summary>
public class AddPlayerToHockeyTeamRequest
{
    /// <summary>Hockey player id.</summary>
    [Required]
    public Guid PlayerId { get; set; }

    /// <summary>Roster position.</summary>
    [Required]
    public HockeyPosition Position { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }

    /// <summary>Optional jersey number.</summary>
    public int? JerseyNumber { get; set; }

    /// <summary>Optional requested jersey number.</summary>
    public int? RequestedJerseyNumber { get; set; }

    /// <summary>Roster status.</summary>
    public HockeyRosterStatus RosterStatus { get; set; } = HockeyRosterStatus.Active;
}

/// <summary>
/// Request body for updating a hockey team roster membership.
/// </summary>
public class UpdateHockeyTeamPlayerRequest
{
    /// <summary>Roster position.</summary>
    [Required]
    public HockeyPosition Position { get; set; }

    /// <summary>Jersey number.</summary>
    public int? JerseyNumber { get; set; }

    /// <summary>Roster status.</summary>
    [Required]
    public HockeyRosterStatus RosterStatus { get; set; }

    /// <summary>Captain role.</summary>
    [Required]
    public HockeyCaptainRole CaptainRole { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
}

/// <summary>
/// Request body for adding a hockey line.
/// </summary>
public class AddHockeyLineRequest
{
    /// <summary>Line display name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Line number.</summary>
    [Required]
    public int LineNumber { get; set; }

    /// <summary>Line type.</summary>
    [Required]
    public HockeyLineType LineType { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
}

/// <summary>
/// Request body for placing a team player on a line.
/// </summary>
public class AddPlayerToHockeyLineRequest
{
    /// <summary>Team-player membership id.</summary>
    [Required]
    public Guid TeamPlayerId { get; set; }

    /// <summary>Line slot.</summary>
    [Required]
    public HockeyLineSlot Slot { get; set; }

    /// <summary>Order within the line.</summary>
    [Required]
    public int Order { get; set; }
}

/// <summary>
/// Request body for adding staff to a hockey team.
/// </summary>
public class AddHockeyTeamStaffRequest
{
    /// <summary>Common Person id.</summary>
    [Required]
    public Guid PersonId { get; set; }

    /// <summary>Staff role.</summary>
    [Required]
    public HockeyTeamStaffRole Role { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
}
