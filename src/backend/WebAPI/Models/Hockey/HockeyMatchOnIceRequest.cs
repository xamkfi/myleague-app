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
/// Request body for putting a player on ice.
/// </summary>
public class AddHockeyMatchPlayerToIceRequest
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
    public HockeyIceSlot? Slot { get; set; }
    /// <summary>
    /// Order.
    /// </summary>
    public int? Order { get; set; }
    /// <summary>
    /// Is Goalie.
    /// </summary>
    public bool? IsGoalie { get; set; }
    /// <summary>
    /// Is Extra Attacker.
    /// </summary>
    public bool IsExtraAttacker { get; set; }
    /// <summary>
    /// Period Number.
    /// </summary>
    public int? PeriodNumber { get; set; }
    /// <summary>
    /// Time In Seconds.
    /// </summary>
    public int? TimeInSeconds { get; set; }
    /// <summary>
    /// User Id.
    /// </summary>
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for removing a player from ice.
/// </summary>
public class RemoveHockeyMatchPlayerFromIceRequest
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
    /// Period Number.
    /// </summary>
    public int? PeriodNumber { get; set; }
    /// <summary>
    /// Time In Seconds.
    /// </summary>
    public int? TimeInSeconds { get; set; }
    /// <summary>
    /// User Id.
    /// </summary>
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for clearing ice / applying line.
/// </summary>
public class HockeyMatchIceActionRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    public int? PeriodNumber { get; set; }
    /// <summary>
    /// Time In Seconds.
    /// </summary>
    public int? TimeInSeconds { get; set; }
    /// <summary>
    /// User Id.
    /// </summary>
    public Guid? UserId { get; set; }
}
