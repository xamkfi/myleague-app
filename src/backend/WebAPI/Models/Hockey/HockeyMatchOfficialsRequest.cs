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
/// Request body for assigning an official to a match.
/// </summary>
public class AddHockeyMatchOfficialRequest
{
    /// <summary>
    /// Official Id.
    /// </summary>
    [Required]
    public Guid OfficialId { get; set; }

    /// <summary>
    /// Role.
    /// </summary>
    [Required]
    public HockeyOfficialRole Role { get; set; }

    /// <summary>
    /// Is Main Official.
    /// </summary>
    public bool IsMainOfficial { get; set; }
}
