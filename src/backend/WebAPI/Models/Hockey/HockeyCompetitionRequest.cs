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

    /// <summary>Copy the latest roster into this competition, or start empty.</summary>
    public Domain.Enums.Common.RosterEnrollmentMode RosterMode { get; set; } =
        Domain.Enums.Common.RosterEnrollmentMode.CopyLatest;
}

/// <summary>
/// Request body for updating shared hockey competition rules.
/// Nested rule sections default when omitted.
/// </summary>
public class UpdateHockeyCompetitionRulesRequest
{
    /// <summary>Rules display name.</summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional rule book version.</summary>
    [StringLength(50)]
    public string? RuleBookVersion { get; set; }

    /// <summary>Rule book source.</summary>
    [Required]
    public HockeyRuleBookSource RuleBookSource { get; set; }

    /// <summary>
    /// Match Rules.
    /// </summary>
    public HockeyMatchRulesInputDto? MatchRules { get; set; }
    /// <summary>
    /// Standing Rules.
    /// </summary>
    public HockeyStandingRulesInputDto? StandingRules { get; set; }
    /// <summary>
    /// Roster Rules.
    /// </summary>
    public HockeyRosterRulesInputDto? RosterRules { get; set; }
    /// <summary>
    /// Video Review Rules.
    /// </summary>
    public HockeyVideoReviewRulesInputDto? VideoReviewRules { get; set; }
    /// <summary>
    /// Contact Rules.
    /// </summary>
    public HockeyContactRulesInputDto? ContactRules { get; set; }
}

/// <summary>
/// Request body for recalculating competition hockey statistics.
/// </summary>
public class RecalculateHockeyCompetitionStatisticsRequest
{
    /// <summary>
    /// Statistics scope to recalculate. Defaults to the whole competition.
    /// </summary>
    public HockeyStatisticsScope Scope { get; set; } = HockeyStatisticsScope.Competition;

    /// <summary>
    /// Optional season division to limit the recalculation.
    /// </summary>
    public Guid? CompetitionDivisionId { get; set; }

    /// <summary>
    /// Optional tournament group to limit the recalculation.
    /// </summary>
    public Guid? TournamentGroupId { get; set; }

    /// <summary>
    /// Optional playoff series to limit the recalculation.
    /// </summary>
    public Guid? PlayoffSeriesId { get; set; }
}

/// <summary>
/// Request body for resetting competition hockey statistics.
/// </summary>
public class ResetHockeyCompetitionStatisticsRequest
{
    /// <summary>
    /// Statistics scope to reset. When omitted, the handler uses the competition default.
    /// </summary>
    public HockeyStatisticsScope? Scope { get; set; }

    /// <summary>
    /// Optional season division to limit the reset.
    /// </summary>
    public Guid? CompetitionDivisionId { get; set; }

    /// <summary>
    /// Optional tournament group to limit the reset.
    /// </summary>
    public Guid? TournamentGroupId { get; set; }

    /// <summary>
    /// Optional playoff series to limit the reset.
    /// </summary>
    public Guid? PlayoffSeriesId { get; set; }
}
