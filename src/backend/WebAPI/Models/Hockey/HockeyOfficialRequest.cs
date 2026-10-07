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
/// Request body for creating a hockey official profile.
/// </summary>
public class CreateHockeyOfficialRequest
{
    /// <summary>Common Person id.</summary>
    [Required]
    public Guid PersonId { get; set; }

    /// <summary>Official role.</summary>
    [Required]
    public HockeyOfficialRole OfficialRole { get; set; }

    /// <summary>Optional official number / badge.</summary>
    [StringLength(50)]
    public string? OfficialNumber { get; set; }

    /// <summary>Optional license issue date.</summary>
    public DateTime? LicenseIssueDate { get; set; }

    /// <summary>Optional license expiry date.</summary>
    public DateTime? LicenseExpiryDate { get; set; }
}

/// <summary>
/// Request body for updating a hockey official profile.
/// </summary>
public class UpdateHockeyOfficialRequest
{
    /// <summary>Official role.</summary>
    [Required]
    public HockeyOfficialRole OfficialRole { get; set; }

    /// <summary>Optional official number / badge.</summary>
    [StringLength(50)]
    public string? OfficialNumber { get; set; }

    /// <summary>Optional license issue date.</summary>
    public DateTime? LicenseIssueDate { get; set; }

    /// <summary>Optional license expiry date.</summary>
    public DateTime? LicenseExpiryDate { get; set; }

    /// <summary>Whether the official is active.</summary>
    [Required]
    public bool IsActive { get; set; } = true;
}
