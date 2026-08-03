using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

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
