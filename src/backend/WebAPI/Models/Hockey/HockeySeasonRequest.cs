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
/// Request body for creating a hockey season.
/// </summary>
public class CreateHockeySeasonRequest
{
    /// <summary>
    /// Name of the season.
    /// </summary>
    [Required]
    [StringLength(100)]
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

    /// <summary>
    /// Audience / age-group category (Adult, Youth, Women).
    /// </summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;

    /// <summary>
    /// Optional public logo URL for the season hero.
    /// </summary>
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// How many teams advance. Zero hides the highlight.
    /// </summary>
    [Range(0, 999)]
    public int TeamsAdvancing { get; set; }

    /// <summary>
    /// Ordered ranking criteria. Omitted values keep the default order.
    /// </summary>
    public List<StandingSortCriterion>? RankingCriteria { get; set; }

    /// <summary>Points for a win in regulation time.</summary>
    [Range(0, 100)]
    public int RegulationWinPoints { get; set; } = 3;

    /// <summary>Points for a win in overtime.</summary>
    [Range(0, 100)]
    public int OvertimeWinPoints { get; set; } = 2;

    /// <summary>Points for a win in a shootout.</summary>
    [Range(0, 100)]
    public int ShootoutWinPoints { get; set; } = 2;

    /// <summary>Points for a loss in overtime.</summary>
    [Range(0, 100)]
    public int OvertimeLossPoints { get; set; } = 1;

    /// <summary>Points for a loss in a shootout.</summary>
    [Range(0, 100)]
    public int ShootoutLossPoints { get; set; } = 1;

    /// <summary>Points for a tie.</summary>
    [Range(0, 100)]
    public int TiePoints { get; set; } = 1;
}

/// <summary>
/// Request body for updating a hockey season.
/// </summary>
public class UpdateHockeySeasonRequest
{
    /// <summary>Season name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Start date.</summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>End date.</summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>Optional short season code.</summary>
    [StringLength(50)]
    public string? SeasonCode { get; set; }

    /// <summary>Audience / age-group category.</summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;

    /// <summary>Optional public logo URL for the season hero.</summary>
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// How many teams advance. Zero hides the highlight.
    /// </summary>
    [Range(0, 999)]
    public int TeamsAdvancing { get; set; }

    /// <summary>
    /// Ordered ranking criteria. Omitted values keep the current order.
    /// </summary>
    public List<StandingSortCriterion>? RankingCriteria { get; set; }

    /// <summary>Points for a win in regulation time.</summary>
    [Range(0, 100)]
    public int RegulationWinPoints { get; set; } = 3;

    /// <summary>Points for a win in overtime.</summary>
    [Range(0, 100)]
    public int OvertimeWinPoints { get; set; } = 2;

    /// <summary>Points for a win in a shootout.</summary>
    [Range(0, 100)]
    public int ShootoutWinPoints { get; set; } = 2;

    /// <summary>Points for a loss in overtime.</summary>
    [Range(0, 100)]
    public int OvertimeLossPoints { get; set; } = 1;

    /// <summary>Points for a loss in a shootout.</summary>
    [Range(0, 100)]
    public int ShootoutLossPoints { get; set; } = 1;

    /// <summary>Points for a tie.</summary>
    [Range(0, 100)]
    public int TiePoints { get; set; } = 1;
}

/// <summary>
/// One season intro block in a replace-all payload.
/// </summary>
public class HockeySeasonContentBlockItemRequest
{
    /// <summary>Existing block id. Omit to create a new block.</summary>
    public Guid? Id { get; set; }

    /// <summary>Card title shown on public pages.</summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>HTML body produced by the rich-text editor.</summary>
    [StringLength(50000)]
    public string ContentHtml { get; set; } = string.Empty;
}

/// <summary>
/// Replace-all request for a season's intro blocks. Array order is the display order.
/// </summary>
public class ReplaceHockeySeasonContentBlocksRequest
{
    /// <summary>Intro blocks in display order.</summary>
    [Required]
    public List<HockeySeasonContentBlockItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request body for setting the season champion.
/// </summary>
public class SetHockeySeasonChampionRequest
{
    /// <summary>Champion competition-team id.</summary>
    [Required]
    public Guid ChampionCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for adding a Common Division to a hockey season.
/// </summary>
public class AddDivisionToHockeySeasonRequest
{
    /// <summary>Common Division id.</summary>
    [Required]
    public Guid DivisionId { get; set; }

    /// <summary>Display name within the season.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Sort order among sibling divisions.</summary>
    [Required]
    public int SortOrder { get; set; }
}

/// <summary>
/// Request body for placing a competition team into a season division.
/// </summary>
public class AddTeamToHockeySeasonDivisionRequest
{
    /// <summary>Competition-team id (not raw HockeyTeam id).</summary>
    [Required]
    public Guid CompetitionTeamId { get; set; }

    /// <summary>Optional seed within the division.</summary>
    public int? Seed { get; set; }
}
