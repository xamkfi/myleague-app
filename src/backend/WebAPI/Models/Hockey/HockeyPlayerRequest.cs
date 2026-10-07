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
/// Request body for creating a hockey player.
/// </summary>
public class CreateHockeyPlayerRequest
{
    /// <summary>Common Person id.</summary>
    [Required]
    public Guid PersonId { get; set; }

    /// <summary>Primary position.</summary>
    [Required]
    public HockeyPosition PrimaryPosition { get; set; }

    /// <summary>Shooting side.</summary>
    public HockeyShoots Shoots { get; set; } = HockeyShoots.Unknown;

    /// <summary>Catching side (goalies).</summary>
    public HockeyCatches? Catches { get; set; }

    /// <summary>Optional license number.</summary>
    [StringLength(50)]
    public string? LicenseNumber { get; set; }
}
