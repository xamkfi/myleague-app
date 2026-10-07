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
/// Request body for confirming a match-side roster.
/// </summary>
public class ConfirmHockeyMatchRosterRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Team Player Ids.
    /// </summary>
    [Required]
    public List<Guid> TeamPlayerIds { get; set; } = new();

    /// <summary>
    /// Confirmed By User Id.
    /// </summary>
    public Guid? ConfirmedByUserId { get; set; }

    /// <summary>
    /// Source.
    /// </summary>
    public HockeyPlayerSelectionSource Source { get; set; } = HockeyPlayerSelectionSource.Manual;
}

/// <summary>
/// Request body for a club admin announcing a hockey match-day roster.
/// </summary>
public class AnnounceHockeyMatchRosterRequest
{
    /// <summary>
    /// Team-player membership ids to dress for the match.
    /// </summary>
    [Required]
    public List<Guid> TeamPlayerIds { get; set; } = new();
}

/// <summary>
/// Request body identifying a match team (shared).
/// </summary>
public class HockeyMatchTeamIdRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// User Id.
    /// </summary>
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for setting active goalie or deactivating a roster player.
/// </summary>
public class HockeyMatchTeamPlayerRequest
{
    /// <summary>
    /// Match-team row that owns the roster player.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Active roster player to set as goalie or deactivate.
    /// </summary>
    [Required]
    public Guid MatchActivePlayerId { get; set; }
}
