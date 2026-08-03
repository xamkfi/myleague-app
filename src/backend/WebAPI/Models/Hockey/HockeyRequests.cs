using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Hockey;

public class CreateHockeySeasonRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(50)]
    public string? SeasonCode { get; set; }
}

public class CreateHockeyTournamentRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(200)]
    public string? Venue { get; set; }

    public string? ContentHtml { get; set; }
}

public class CreateHockeyTeamRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid ClubId { get; set; }

    [Required]
    public TeamCategory TeamCategory { get; set; }

    public Guid? DivisionId { get; set; }

    [StringLength(200)]
    public string? HomeArena { get; set; }

    [StringLength(50)]
    public string? PrimaryJerseyColor { get; set; }

    [StringLength(50)]
    public string? SecondaryJerseyColor { get; set; }

    [StringLength(50)]
    public string? ShortName { get; set; }
}

public class AddTeamToHockeyCompetitionRequest
{
    [Required]
    public Guid TeamId { get; set; }

    public int? Seed { get; set; }
}
