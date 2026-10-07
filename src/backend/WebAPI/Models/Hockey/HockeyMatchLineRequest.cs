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
/// Request body for adding a match line.
/// </summary>
public class AddHockeyMatchLineRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Line display name.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Line Type.
    /// </summary>
    [Required]
    public HockeyLineType LineType { get; set; }

    /// <summary>
    /// Line Number.
    /// </summary>
    public int? LineNumber { get; set; }
    /// <summary>
    /// Notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for adding a player to a match line.
/// </summary>
public class AddHockeyMatchLinePlayerRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Match Active Player Id.
    /// </summary>
    [Required]
    public Guid MatchActivePlayerId { get; set; }

    /// <summary>
    /// Slot.
    /// </summary>
    public HockeyLineSlot? Slot { get; set; }
    /// <summary>
    /// Order.
    /// </summary>
    public int? Order { get; set; }
}

/// <summary>
/// Request body for updating a match line name.
/// </summary>
public class UpdateHockeyMatchLineNameRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Line display name.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request body for updating match line notes.
/// </summary>
public class UpdateHockeyMatchLineNotesRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public string? Notes { get; set; }
}
