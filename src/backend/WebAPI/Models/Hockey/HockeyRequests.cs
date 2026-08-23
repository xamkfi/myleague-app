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

    /// <summary>
    /// Audience / age-group category (Adult, Youth, Women).
    /// </summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;
}

/// <summary>
/// Request body for updating a hockey season.
/// </summary>
public class UpdateHockeySeasonRequest
{
    /// <summary>Season name.</summary>
    [Required]
    [StringLength(200)]
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

    /// <summary>
    /// Audience / age-group category (Adult, Youth, Women).
    /// </summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;
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
    [StringLength(100)]
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
    [StringLength(4)]
    public string? ShortName { get; set; }
}

/// <summary>
/// Request body for updating a hockey team.
/// </summary>
public class UpdateHockeyTeamRequest
{
    /// <summary>Team name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional short name.</summary>
    [StringLength(4)]
    public string? ShortName { get; set; }

    /// <summary>Team category.</summary>
    [Required]
    public TeamCategory TeamCategory { get; set; }

    /// <summary>Optional division id.</summary>
    public Guid? DivisionId { get; set; }

    /// <summary>Optional home arena.</summary>
    [StringLength(200)]
    public string? HomeArena { get; set; }

    /// <summary>Optional primary jersey color.</summary>
    [StringLength(50)]
    public string? PrimaryJerseyColor { get; set; }

    /// <summary>Optional secondary jersey color.</summary>
    [StringLength(50)]
    public string? SecondaryJerseyColor { get; set; }
}

/// <summary>
/// Request body for setting team active status.
/// </summary>
public class SetHockeyTeamActiveStatusRequest
{
    /// <summary>Whether the team is active.</summary>
    [Required]
    public bool IsActive { get; set; }
}

/// <summary>
/// Request body for updating a team logo URL.
/// </summary>
public class UpdateHockeyTeamLogoRequest
{
    /// <summary>Absolute logo URL, or null to clear.</summary>
    public string? LogoUrl { get; set; }
}

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

/// <summary>
/// Request body for adding a player to a hockey team roster.
/// </summary>
public class AddPlayerToHockeyTeamRequest
{
    /// <summary>Hockey player id.</summary>
    [Required]
    public Guid PlayerId { get; set; }

    /// <summary>Roster position.</summary>
    [Required]
    public HockeyPosition Position { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }

    /// <summary>Optional jersey number.</summary>
    public int? JerseyNumber { get; set; }

    /// <summary>Optional requested jersey number.</summary>
    public int? RequestedJerseyNumber { get; set; }

    /// <summary>Roster status.</summary>
    public HockeyRosterStatus RosterStatus { get; set; } = HockeyRosterStatus.Active;
}

/// <summary>
/// Request body for updating a hockey team roster membership.
/// </summary>
public class UpdateHockeyTeamPlayerRequest
{
    /// <summary>Roster position.</summary>
    [Required]
    public HockeyPosition Position { get; set; }

    /// <summary>Jersey number.</summary>
    public int? JerseyNumber { get; set; }

    /// <summary>Roster status.</summary>
    [Required]
    public HockeyRosterStatus RosterStatus { get; set; }

    /// <summary>Captain role.</summary>
    [Required]
    public HockeyCaptainRole CaptainRole { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
}

/// <summary>
/// Request body for adding a hockey line.
/// </summary>
public class AddHockeyLineRequest
{
    /// <summary>Line display name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Line number.</summary>
    [Required]
    public int LineNumber { get; set; }

    /// <summary>Line type.</summary>
    [Required]
    public HockeyLineType LineType { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
}

/// <summary>
/// Request body for placing a team player on a line.
/// </summary>
public class AddPlayerToHockeyLineRequest
{
    /// <summary>Team-player membership id.</summary>
    [Required]
    public Guid TeamPlayerId { get; set; }

    /// <summary>Line slot.</summary>
    [Required]
    public HockeyLineSlot Slot { get; set; }

    /// <summary>Order within the line.</summary>
    [Required]
    public int Order { get; set; }
}

/// <summary>
/// Request body for adding staff to a hockey team.
/// </summary>
public class AddHockeyTeamStaffRequest
{
    /// <summary>Common Person id.</summary>
    [Required]
    public Guid PersonId { get; set; }

    /// <summary>Staff role.</summary>
    [Required]
    public HockeyTeamStaffRole Role { get; set; }

    /// <summary>Optional competition scope.</summary>
    public Guid? CompetitionId { get; set; }
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
/// Request body for creating a hockey tournament group (lohko).
/// </summary>
public class CreateHockeyTournamentGroupRequest
{
    /// <summary>
    /// Group display name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request body for adding a competition team to a hockey tournament group.
/// </summary>
public class AddTeamToHockeyTournamentGroupRequest
{
    /// <summary>
    /// Competition-team membership id (not raw HockeyTeam id).
    /// </summary>
    [Required]
    public Guid CompetitionTeamId { get; set; }

    /// <summary>
    /// Optional seed within the group.
    /// </summary>
    public int? Seed { get; set; }
}

/// <summary>
/// Request body for updating a hockey tournament.
/// </summary>
public class UpdateHockeyTournamentRequest
{
    /// <summary>Tournament name.</summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Start date.</summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>End date.</summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>Optional venue.</summary>
    [StringLength(200)]
    public string? Venue { get; set; }

    /// <summary>Optional HTML content.</summary>
    public string? ContentHtml { get; set; }

    /// <summary>Audience / age-group category.</summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;
}

/// <summary>
/// Request body for updating hockey tournament rules.
/// </summary>
public class UpdateHockeyTournamentRulesRequest
{
    /// <summary>Tournament format.</summary>
    [Required]
    public HockeyTournamentFormat Format { get; set; }

    /// <summary>Whether group stage is enabled.</summary>
    public bool HasGroupStage { get; set; }

    /// <summary>Whether playoffs are enabled.</summary>
    public bool HasPlayoffs { get; set; }

    /// <summary>Whether bronze game is enabled.</summary>
    public bool HasBronzeGame { get; set; }

    /// <summary>Whether placement games are enabled.</summary>
    public bool HasPlacementGames { get; set; }

    /// <summary>Teams advancing per group when playoffs are enabled.</summary>
    public int TeamsAdvancingPerGroup { get; set; }
}

/// <summary>
/// Request body for setting the tournament champion.
/// </summary>
public class SetHockeyTournamentChampionRequest
{
    /// <summary>Champion competition-team id.</summary>
    [Required]
    public Guid ChampionCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for creating a playoff series.
/// </summary>
public class CreateHockeyPlayoffSeriesRequest
{
    /// <summary>Playoff round.</summary>
    [Required]
    public HockeyPlayoffRound Round { get; set; }

    /// <summary>Order within the round.</summary>
    [Required]
    public int SeriesOrder { get; set; }

    /// <summary>Best-of value (minimum 1).</summary>
    [Required]
    public int BestOf { get; set; }

    /// <summary>Optional home competition-team id.</summary>
    public Guid? HomeCompetitionTeamId { get; set; }

    /// <summary>Optional away competition-team id.</summary>
    public Guid? AwayCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for assigning teams to a playoff series.
/// </summary>
public class AssignHockeyPlayoffSeriesTeamsRequest
{
    /// <summary>Home competition-team id.</summary>
    [Required]
    public Guid HomeCompetitionTeamId { get; set; }

    /// <summary>Away competition-team id.</summary>
    [Required]
    public Guid AwayCompetitionTeamId { get; set; }
}

/// <summary>
/// Request body for replacing the tournament playoff schedule.
/// </summary>
public class SetHockeyTournamentPlayoffScheduleRequest
{
    /// <summary>Schedule slots.</summary>
    [Required]
    public List<HockeyPlayoffScheduleSlotDto> Slots { get; set; } = new();
}

/// <summary>
/// Request body for creating a hockey match.
/// </summary>
public class CreateHockeyMatchRequest
{
    /// <summary>
    /// Scheduled Start Time.
    /// </summary>
    [Required]
    public DateTime ScheduledStartTime { get; set; }

    /// <summary>
    /// Match Type.
    /// </summary>
    [Required]
    public HockeyMatchType MatchType { get; set; }

    /// <summary>
    /// Competition Id.
    /// </summary>
    public Guid? CompetitionId { get; set; }
    /// <summary>
    /// Competition Division Id.
    /// </summary>
    public Guid? CompetitionDivisionId { get; set; }
    /// <summary>
    /// Tournament Group Id.
    /// </summary>
    public Guid? TournamentGroupId { get; set; }
    /// <summary>
    /// Playoff Series Id.
    /// </summary>
    public Guid? PlayoffSeriesId { get; set; }

    /// <summary>
    /// Venue.
    /// </summary>
    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for assigning home/away teams to a hockey match.
/// </summary>
public class AddHomeAwayTeamsToHockeyMatchRequest
{
    /// <summary>
    /// Home Team Id.
    /// </summary>
    [Required]
    public Guid HomeTeamId { get; set; }

    /// <summary>
    /// Away Team Id.
    /// </summary>
    [Required]
    public Guid AwayTeamId { get; set; }
}

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
/// Request body for recording a hockey goal.
/// </summary>
public class RecordHockeyGoalRequest
{
    /// <summary>
    /// Scoring Match Team Id.
    /// </summary>
    [Required]
    public Guid ScoringMatchTeamId { get; set; }

    /// <summary>
    /// Scorer Active Player Id.
    /// </summary>
    [Required]
    public Guid ScorerActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Goal Strength.
    /// </summary>
    [Required]
    public HockeyGoalStrength GoalStrength { get; set; }

    /// <summary>
    /// Primary Assist Active Player Id.
    /// </summary>
    public Guid? PrimaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Secondary Assist Active Player Id.
    /// </summary>
    public Guid? SecondaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Was Empty Net.
    /// </summary>
    public bool WasEmptyNet { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for correcting a hockey goal during live match operations.
/// </summary>
public class UpdateHockeyGoalRequest
{
    /// <summary>
    /// Scoring Match Team Id.
    /// </summary>
    [Required]
    public Guid ScoringMatchTeamId { get; set; }

    /// <summary>
    /// Scorer Active Player Id.
    /// </summary>
    [Required]
    public Guid ScorerActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Goal Strength.
    /// </summary>
    [Required]
    public HockeyGoalStrength GoalStrength { get; set; }

    /// <summary>
    /// Primary Assist Active Player Id.
    /// </summary>
    public Guid? PrimaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Secondary Assist Active Player Id.
    /// </summary>
    public Guid? SecondaryAssistActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Was Empty Net.
    /// </summary>
    public bool WasEmptyNet { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey penalty.
/// </summary>
public class RecordHockeyPenaltyRequest
{
    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Severity.
    /// </summary>
    [Required]
    public HockeyPenaltySeverity Severity { get; set; }

    /// <summary>
    /// Offence.
    /// </summary>
    [Required]
    public HockeyPenaltyOffence Offence { get; set; }

    /// <summary>
    /// Penalty Minutes.
    /// </summary>
    [Required]
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Penalized Active Player Id.
    /// </summary>
    public Guid? PenalizedActivePlayerId { get; set; }
    /// <summary>
    /// Served By Active Player Id.
    /// </summary>
    public Guid? ServedByActivePlayerId { get; set; }
    /// <summary>
    /// Is Bench Penalty.
    /// </summary>
    public bool IsBenchPenalty { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for correcting a hockey penalty during live match operations.
/// </summary>
public class UpdateHockeyPenaltyRequest
{
    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Severity.
    /// </summary>
    [Required]
    public HockeyPenaltySeverity Severity { get; set; }

    /// <summary>
    /// Offence.
    /// </summary>
    [Required]
    public HockeyPenaltyOffence Offence { get; set; }

    /// <summary>
    /// Penalty Minutes.
    /// </summary>
    [Required]
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Penalized Active Player Id.
    /// </summary>
    public Guid? PenalizedActivePlayerId { get; set; }
    /// <summary>
    /// Served By Active Player Id.
    /// </summary>
    public Guid? ServedByActivePlayerId { get; set; }
    /// <summary>
    /// Is Bench Penalty.
    /// </summary>
    public bool IsBenchPenalty { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey shot.
/// </summary>
public class RecordHockeyShotRequest
{
    /// <summary>
    /// Shooting Match Team Id.
    /// </summary>
    [Required]
    public Guid ShootingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Result.
    /// </summary>
    [Required]
    public HockeyShotResult ShotResult { get; set; }

    /// <summary>
    /// Counts As Shot On Goal.
    /// </summary>
    public bool CountsAsShotOnGoal { get; set; } = true;
    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    public Guid? ShooterActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for correcting a hockey shot during live match operations.
/// </summary>
public class UpdateHockeyShotRequest
{
    /// <summary>
    /// Shooting Match Team Id.
    /// </summary>
    [Required]
    public Guid ShootingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Result.
    /// </summary>
    [Required]
    public HockeyShotResult ShotResult { get; set; }

    /// <summary>
    /// Counts As Shot On Goal.
    /// </summary>
    public bool CountsAsShotOnGoal { get; set; } = true;
    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    public Guid? ShooterActivePlayerId { get; set; }
    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    public Guid? GoalieActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey video review.
/// </summary>
public class RecordHockeyVideoReviewRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Review Type.
    /// </summary>
    [Required]
    public HockeyVideoReviewType ReviewType { get; set; }

    /// <summary>
    /// Original Decision.
    /// </summary>
    [Required]
    public HockeyReviewDecision OriginalDecision { get; set; }

    /// <summary>
    /// Final Decision.
    /// </summary>
    [Required]
    public HockeyReviewDecision FinalDecision { get; set; }

    /// <summary>
    /// Is Coach Challenge.
    /// </summary>
    public bool IsCoachChallenge { get; set; }
    /// <summary>
    /// Was Successful.
    /// </summary>
    public bool WasSuccessful { get; set; }
    /// <summary>
    /// Requested By Match Team Id.
    /// </summary>
    public Guid? RequestedByMatchTeamId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as started.
/// </summary>
public class MarkHockeyMatchStartedRequest
{
    /// <summary>
    /// Actual Start Time.
    /// </summary>
    public DateTime? ActualStartTime { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as finished.
/// </summary>
public class MarkHockeyMatchFinishedRequest
{
    /// <summary>
    /// Actual End Time.
    /// </summary>
    public DateTime? ActualEndTime { get; set; }
    /// <summary>
    /// Result Type.
    /// </summary>
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting hockey match status.
/// </summary>
public class SetHockeyMatchStatusRequest
{
    /// <summary>
    /// Status.
    /// </summary>
    [Required]
    public HockeyMatchStatus Status { get; set; }
}

/// <summary>
/// Request body for setting hockey match result type.
/// </summary>
public class SetHockeyMatchResultTypeRequest
{
    /// <summary>
    /// Result Type.
    /// </summary>
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting the current period.
/// </summary>
public class SetHockeyMatchCurrentPeriodRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }
}

/// <summary>
/// Request body for overtime / shootout flags.
/// </summary>
public class SetHockeyMatchBooleanFlagRequest
{
    /// <summary>
    /// Value.
    /// </summary>
    [Required]
    public bool Value { get; set; }
}

/// <summary>
/// Request body for updating match venue.
/// </summary>
public class UpdateHockeyMatchVenueRequest
{
    /// <summary>
    /// Venue.
    /// </summary>
    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for updating scheduled start.
/// </summary>
public class UpdateHockeyMatchScheduledStartRequest
{
    /// <summary>
    /// Scheduled Start Time.
    /// </summary>
    [Required]
    public DateTime ScheduledStartTime { get; set; }
}

/// <summary>
/// Request body for correcting team goals.
/// </summary>
public class SetHockeyMatchTeamGoalsRequest
{
    /// <summary>
    /// Team Slot.
    /// </summary>
    [Required]
    public HockeyTeamSlot TeamSlot { get; set; }

    /// <summary>
    /// Goals.
    /// </summary>
    [Required]
    public int Goals { get; set; }
}

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

/// <summary>
/// Request body for creating a period score row.
/// </summary>
public class AddHockeyPeriodScoreRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Period Type.
    /// </summary>
    [Required]
    public HockeyPeriodType PeriodType { get; set; }
}

/// <summary>
/// Request body for recording a period event.
/// </summary>
public class RecordHockeyPeriodEventRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Action.
    /// </summary>
    [Required]
    public HockeyPeriodAction Action { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a faceoff.
/// </summary>
public class RecordHockeyFaceoffRequest
{
    /// <summary>
    /// Winning Match Team Id.
    /// </summary>
    [Required]
    public Guid WinningMatchTeamId { get; set; }

    /// <summary>
    /// Losing Match Team Id.
    /// </summary>
    [Required]
    public Guid LosingMatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Zone.
    /// </summary>
    [Required]
    public HockeyFaceoffZone Zone { get; set; }

    /// <summary>
    /// Spot.
    /// </summary>
    [Required]
    public HockeyFaceoffSpot Spot { get; set; }

    /// <summary>
    /// Winning Active Player Id.
    /// </summary>
    public Guid? WinningActivePlayerId { get; set; }
    /// <summary>
    /// Losing Active Player Id.
    /// </summary>
    public Guid? LosingActivePlayerId { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a stoppage.
/// </summary>
public class RecordHockeyStoppageRequest
{
    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Reason.
    /// </summary>
    [Required]
    public HockeyStoppageReason Reason { get; set; }

    /// <summary>
    /// Responsible Match Team Id.
    /// </summary>
    public Guid? ResponsibleMatchTeamId { get; set; }
    /// <summary>
    /// Responsible Active Player Id.
    /// </summary>
    public Guid? ResponsibleActivePlayerId { get; set; }
    /// <summary>
    /// Next Faceoff Zone.
    /// </summary>
    public HockeyFaceoffZone? NextFaceoffZone { get; set; }
    /// <summary>
    /// Next Faceoff Spot.
    /// </summary>
    public HockeyFaceoffSpot? NextFaceoffSpot { get; set; }
    /// <summary>
    /// Rule Reference.
    /// </summary>
    public string? RuleReference { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a timeout.
/// </summary>
public class RecordHockeyTimeoutRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a goalie change.
/// </summary>
public class RecordHockeyGoalieChangeRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Outgoing Goalie Active Player Id.
    /// </summary>
    public Guid? OutgoingGoalieActivePlayerId { get; set; }
    /// <summary>
    /// Incoming Goalie Active Player Id.
    /// </summary>
    public Guid? IncomingGoalieActivePlayerId { get; set; }
    /// <summary>
    /// Reason.
    /// </summary>
    public string? Reason { get; set; }
    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a shootout attempt.
/// </summary>
public class RecordHockeyShootoutAttemptRequest
{
    /// <summary>
    /// Match Team Id.
    /// </summary>
    [Required]
    public Guid MatchTeamId { get; set; }

    /// <summary>
    /// Shooter Active Player Id.
    /// </summary>
    [Required]
    public Guid ShooterActivePlayerId { get; set; }

    /// <summary>
    /// Goalie Active Player Id.
    /// </summary>
    [Required]
    public Guid GoalieActivePlayerId { get; set; }

    /// <summary>
    /// Period Number.
    /// </summary>
    [Required]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Time In Seconds.
    /// </summary>
    [Required]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Shot Order.
    /// </summary>
    [Required]
    public int ShotOrder { get; set; }

    /// <summary>
    /// Result.
    /// </summary>
    [Required]
    public HockeyShootoutAttemptResult Result { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a failed coach-challenge penalty.
/// </summary>
public class RecordHockeyFailedCoachChallengePenaltyRequest
{
    /// <summary>
    /// Video Review Id.
    /// </summary>
    [Required]
    public Guid VideoReviewId { get; set; }

    /// <summary>
    /// Penalty Match Team Id.
    /// </summary>
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    /// <summary>
    /// Enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// Max Challenges Per Team.
    /// </summary>
    public int MaxChallengesPerTeam { get; set; } = 1;
    /// <summary>
    /// Lose Challenge After Failed.
    /// </summary>
    public bool LoseChallengeAfterFailed { get; set; } = true;
    /// <summary>
    /// Penalty For Failed Challenge.
    /// </summary>
    public bool PenaltyForFailedChallenge { get; set; } = true;
    /// <summary>
    /// Failed Challenge Penalty Minutes.
    /// </summary>
    public int FailedChallengePenaltyMinutes { get; set; } = 2;
    /// <summary>
    /// Failed Challenge Penalty Offence.
    /// </summary>
    public HockeyPenaltyOffence FailedChallengePenaltyOffence { get; set; } = HockeyPenaltyOffence.DelayOfGame;
    /// <summary>
    /// Failed Challenge Penalty Severity.
    /// </summary>
    public HockeyPenaltySeverity FailedChallengePenaltySeverity { get; set; } = HockeyPenaltySeverity.Minor;
    /// <summary>
    /// Allow Challenge In Overtime.
    /// </summary>
    public bool AllowChallengeInOvertime { get; set; } = true;
    /// <summary>
    /// Allow Challenge In Shootout.
    /// </summary>
    public bool AllowChallengeInShootout { get; set; }
}

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

