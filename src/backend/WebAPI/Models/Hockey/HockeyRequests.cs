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

    public HockeyMatchRulesInputDto? MatchRules { get; set; }
    public HockeyStandingRulesInputDto? StandingRules { get; set; }
    public HockeyRosterRulesInputDto? RosterRules { get; set; }
    public HockeyVideoReviewRulesInputDto? VideoReviewRules { get; set; }
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
    [Required]
    public DateTime ScheduledStartTime { get; set; }

    [Required]
    public HockeyMatchType MatchType { get; set; }

    public Guid? CompetitionId { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }

    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for assigning home/away teams to a hockey match.
/// </summary>
public class AddHomeAwayTeamsToHockeyMatchRequest
{
    [Required]
    public Guid HomeTeamId { get; set; }

    [Required]
    public Guid AwayTeamId { get; set; }
}

/// <summary>
/// Request body for confirming a match-side roster.
/// </summary>
public class ConfirmHockeyMatchRosterRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public List<Guid> TeamPlayerIds { get; set; } = new();

    public Guid? ConfirmedByUserId { get; set; }

    public HockeyPlayerSelectionSource Source { get; set; } = HockeyPlayerSelectionSource.Manual;
}

/// <summary>
/// Request body for recording a hockey goal.
/// </summary>
public class RecordHockeyGoalRequest
{
    [Required]
    public Guid ScoringMatchTeamId { get; set; }

    [Required]
    public Guid ScorerActivePlayerId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyGoalStrength GoalStrength { get; set; }

    public Guid? PrimaryAssistActivePlayerId { get; set; }
    public Guid? SecondaryAssistActivePlayerId { get; set; }
    public Guid? GoalieActivePlayerId { get; set; }
    public bool WasEmptyNet { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey penalty.
/// </summary>
public class RecordHockeyPenaltyRequest
{
    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyPenaltySeverity Severity { get; set; }

    [Required]
    public HockeyPenaltyOffence Offence { get; set; }

    [Required]
    public int PenaltyMinutes { get; set; }

    public Guid? PenalizedActivePlayerId { get; set; }
    public Guid? ServedByActivePlayerId { get; set; }
    public bool IsBenchPenalty { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey shot.
/// </summary>
public class RecordHockeyShotRequest
{
    [Required]
    public Guid ShootingMatchTeamId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyShotResult ShotResult { get; set; }

    public bool CountsAsShotOnGoal { get; set; } = true;
    public Guid? ShooterActivePlayerId { get; set; }
    public Guid? GoalieActivePlayerId { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a hockey video review.
/// </summary>
public class RecordHockeyVideoReviewRequest
{
    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyVideoReviewType ReviewType { get; set; }

    [Required]
    public HockeyReviewDecision OriginalDecision { get; set; }

    [Required]
    public HockeyReviewDecision FinalDecision { get; set; }

    public bool IsCoachChallenge { get; set; }
    public bool WasSuccessful { get; set; }
    public Guid? RequestedByMatchTeamId { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as started.
/// </summary>
public class MarkHockeyMatchStartedRequest
{
    public DateTime? ActualStartTime { get; set; }
}

/// <summary>
/// Request body for marking a hockey match as finished.
/// </summary>
public class MarkHockeyMatchFinishedRequest
{
    public DateTime? ActualEndTime { get; set; }
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting hockey match status.
/// </summary>
public class SetHockeyMatchStatusRequest
{
    [Required]
    public HockeyMatchStatus Status { get; set; }
}

/// <summary>
/// Request body for setting hockey match result type.
/// </summary>
public class SetHockeyMatchResultTypeRequest
{
    public HockeyMatchResultType? ResultType { get; set; }
}

/// <summary>
/// Request body for setting the current period.
/// </summary>
public class SetHockeyMatchCurrentPeriodRequest
{
    [Required]
    public int PeriodNumber { get; set; }
}

/// <summary>
/// Request body for overtime / shootout flags.
/// </summary>
public class SetHockeyMatchBooleanFlagRequest
{
    [Required]
    public bool Value { get; set; }
}

/// <summary>
/// Request body for updating match venue.
/// </summary>
public class UpdateHockeyMatchVenueRequest
{
    [StringLength(200)]
    public string? Venue { get; set; }
}

/// <summary>
/// Request body for updating scheduled start.
/// </summary>
public class UpdateHockeyMatchScheduledStartRequest
{
    [Required]
    public DateTime ScheduledStartTime { get; set; }
}

/// <summary>
/// Request body for correcting team goals.
/// </summary>
public class SetHockeyMatchTeamGoalsRequest
{
    [Required]
    public HockeyTeamSlot TeamSlot { get; set; }

    [Required]
    public int Goals { get; set; }
}

/// <summary>
/// Request body for assigning an official to a match.
/// </summary>
public class AddHockeyMatchOfficialRequest
{
    [Required]
    public Guid OfficialId { get; set; }

    [Required]
    public HockeyOfficialRole Role { get; set; }

    public bool IsMainOfficial { get; set; }
}

/// <summary>
/// Request body for creating a period score row.
/// </summary>
public class AddHockeyPeriodScoreRequest
{
    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public HockeyPeriodType PeriodType { get; set; }
}

/// <summary>
/// Request body for recording a period event.
/// </summary>
public class RecordHockeyPeriodEventRequest
{
    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyPeriodAction Action { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a faceoff.
/// </summary>
public class RecordHockeyFaceoffRequest
{
    [Required]
    public Guid WinningMatchTeamId { get; set; }

    [Required]
    public Guid LosingMatchTeamId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyFaceoffZone Zone { get; set; }

    [Required]
    public HockeyFaceoffSpot Spot { get; set; }

    public Guid? WinningActivePlayerId { get; set; }
    public Guid? LosingActivePlayerId { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a stoppage.
/// </summary>
public class RecordHockeyStoppageRequest
{
    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public HockeyStoppageReason Reason { get; set; }

    public Guid? ResponsibleMatchTeamId { get; set; }
    public Guid? ResponsibleActivePlayerId { get; set; }
    public HockeyFaceoffZone? NextFaceoffZone { get; set; }
    public HockeyFaceoffSpot? NextFaceoffSpot { get; set; }
    public string? RuleReference { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a timeout.
/// </summary>
public class RecordHockeyTimeoutRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a goalie change.
/// </summary>
public class RecordHockeyGoalieChangeRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    public Guid? OutgoingGoalieActivePlayerId { get; set; }
    public Guid? IncomingGoalieActivePlayerId { get; set; }
    public string? Reason { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a shootout attempt.
/// </summary>
public class RecordHockeyShootoutAttemptRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public Guid ShooterActivePlayerId { get; set; }

    [Required]
    public Guid GoalieActivePlayerId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public int TimeInSeconds { get; set; }

    [Required]
    public int ShotOrder { get; set; }

    [Required]
    public HockeyShootoutAttemptResult Result { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// Request body for recording a failed coach-challenge penalty.
/// </summary>
public class RecordHockeyFailedCoachChallengePenaltyRequest
{
    [Required]
    public Guid VideoReviewId { get; set; }

    [Required]
    public Guid PenaltyMatchTeamId { get; set; }

    public bool Enabled { get; set; } = true;
    public int MaxChallengesPerTeam { get; set; } = 1;
    public bool LoseChallengeAfterFailed { get; set; } = true;
    public bool PenaltyForFailedChallenge { get; set; } = true;
    public int FailedChallengePenaltyMinutes { get; set; } = 2;
    public HockeyPenaltyOffence FailedChallengePenaltyOffence { get; set; } = HockeyPenaltyOffence.DelayOfGame;
    public HockeyPenaltySeverity FailedChallengePenaltySeverity { get; set; } = HockeyPenaltySeverity.Minor;
    public bool AllowChallengeInOvertime { get; set; } = true;
    public bool AllowChallengeInShootout { get; set; }
}

/// <summary>
/// Request body for adding a match line.
/// </summary>
public class AddHockeyMatchLineRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public HockeyLineType LineType { get; set; }

    public int? LineNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for adding a player to a match line.
/// </summary>
public class AddHockeyMatchLinePlayerRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public Guid MatchActivePlayerId { get; set; }

    public HockeyLineSlot? Slot { get; set; }
    public int? Order { get; set; }
}

/// <summary>
/// Request body for updating a match line name.
/// </summary>
public class UpdateHockeyMatchLineNameRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request body for updating match line notes.
/// </summary>
public class UpdateHockeyMatchLineNotesRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request body identifying a match team (shared).
/// </summary>
public class HockeyMatchTeamIdRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for putting a player on ice.
/// </summary>
public class AddHockeyMatchPlayerToIceRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public Guid MatchActivePlayerId { get; set; }

    public HockeyIceSlot? Slot { get; set; }
    public int? Order { get; set; }
    public bool? IsGoalie { get; set; }
    public bool IsExtraAttacker { get; set; }
    public int? PeriodNumber { get; set; }
    public int? TimeInSeconds { get; set; }
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for removing a player from ice.
/// </summary>
public class RemoveHockeyMatchPlayerFromIceRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public Guid MatchActivePlayerId { get; set; }

    public int? PeriodNumber { get; set; }
    public int? TimeInSeconds { get; set; }
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for clearing ice / applying line.
/// </summary>
public class HockeyMatchIceActionRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    public int? PeriodNumber { get; set; }
    public int? TimeInSeconds { get; set; }
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request body for setting active goalie or deactivating a roster player.
/// </summary>
public class HockeyMatchTeamPlayerRequest
{
    [Required]
    public Guid MatchTeamId { get; set; }

    [Required]
    public Guid MatchActivePlayerId { get; set; }
}

/// <summary>
/// Request body for recalculating competition hockey statistics.
/// </summary>
public class RecalculateHockeyCompetitionStatisticsRequest
{
    public HockeyStatisticsScope Scope { get; set; } = HockeyStatisticsScope.Competition;
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
}

/// <summary>
/// Request body for resetting competition hockey statistics.
/// </summary>
public class ResetHockeyCompetitionStatisticsRequest
{
    public HockeyStatisticsScope? Scope { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
}

