using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;

namespace Application.Features.Hockey.Competitions.DTOs;

/// <summary>
/// Full hockey competition rules (read model).
/// </summary>
public record HockeyCompetitionRulesDto(
    string Name,
    string? RuleBookVersion,
    string RuleBookSource,
    HockeyMatchRulesDto MatchRules,
    HockeyStandingRulesDto StandingRules,
    HockeyRosterRulesDto RosterRules,
    HockeyVideoReviewRulesDto? VideoReviewRules,
    HockeyContactRulesDto? ContactRules);

/// <summary>
/// Match timing / gameplay rules.
/// </summary>
public record HockeyMatchRulesDto(
    int RegularPeriodCount,
    int RegularPeriodLengthMinutes,
    int OvertimeLengthMinutes,
    bool StopClock,
    bool OvertimeEnabled,
    bool ShootoutEnabled,
    bool OffsideEnabled,
    bool DelayedOffsideEnabled,
    string IcingRule,
    bool PenaltyShotEnabled,
    bool GoaliePullAllowed);

/// <summary>
/// Standing point rules.
/// </summary>
public record HockeyStandingRulesDto(
    int RegulationWinPoints,
    int OvertimeWinPoints,
    int ShootoutWinPoints,
    int OvertimeLossPoints,
    int ShootoutLossPoints,
    int TiePoints,
    IReadOnlyCollection<string> TieBreakers);

/// <summary>
/// Roster composition rules.
/// </summary>
public record HockeyRosterRulesDto(
    int MaxDressedPlayers,
    int MaxDressedGoalies,
    int MinDressedPlayers,
    bool RequiresGoalie,
    int MaxCaptains,
    int MaxAlternateCaptains,
    bool CanGoalieBeCaptain,
    bool AllowGuestPlayers,
    bool LineManagementEnabled);

/// <summary>
/// Video review rules.
/// </summary>
public record HockeyVideoReviewRulesDto(
    bool Enabled,
    bool CoachChallengeAllowed,
    bool ReviewGoals,
    bool ReviewOffsideBeforeGoal,
    bool ReviewGoalieInterference,
    bool ReviewHighStickGoal,
    bool ReviewPuckOverLine,
    HockeyCoachChallengeRulesDto CoachChallengeRules);

/// <summary>
/// Coach challenge rules.
/// </summary>
public record HockeyCoachChallengeRulesDto(
    bool Enabled,
    int MaxChallengesPerTeam,
    bool LoseChallengeAfterFailed,
    bool PenaltyForFailedChallenge,
    int FailedChallengePenaltyMinutes,
    string FailedChallengePenaltyOffence,
    string FailedChallengePenaltySeverity,
    bool AllowChallengeInOvertime,
    bool AllowChallengeInShootout);

/// <summary>
/// Contact rules.
/// </summary>
public record HockeyContactRulesDto(
    bool BodyCheckingAllowed,
    bool OpenIceHitsAllowed,
    bool FightingAllowed,
    bool AutomaticGameMisconductForFight,
    bool StrictHeadContactRule);

/// <summary>
/// Input for replacing competition rules (nested sections default when omitted).
/// </summary>
public record HockeyCompetitionRulesInputDto(
    string Name,
    string? RuleBookVersion,
    HockeyRuleBookSource RuleBookSource,
    HockeyMatchRulesInputDto? MatchRules = null,
    HockeyStandingRulesInputDto? StandingRules = null,
    HockeyRosterRulesInputDto? RosterRules = null,
    HockeyVideoReviewRulesInputDto? VideoReviewRules = null,
    HockeyContactRulesInputDto? ContactRules = null);

public record HockeyMatchRulesInputDto(
    int RegularPeriodCount,
    int RegularPeriodLengthMinutes,
    int OvertimeLengthMinutes,
    bool StopClock,
    bool OvertimeEnabled,
    bool ShootoutEnabled,
    bool OffsideEnabled,
    bool DelayedOffsideEnabled,
    HockeyIcingRule IcingRule,
    bool PenaltyShotEnabled,
    bool GoaliePullAllowed);

public record HockeyStandingRulesInputDto(
    int RegulationWinPoints,
    int OvertimeWinPoints,
    int ShootoutWinPoints,
    int OvertimeLossPoints,
    int ShootoutLossPoints,
    int TiePoints,
    IReadOnlyCollection<HockeyTieBreakerRule>? TieBreakers = null);

public record HockeyRosterRulesInputDto(
    int MaxDressedPlayers,
    int MaxDressedGoalies,
    int MinDressedPlayers,
    bool RequiresGoalie,
    int MaxCaptains,
    int MaxAlternateCaptains,
    bool CanGoalieBeCaptain,
    bool AllowGuestPlayers,
    bool LineManagementEnabled);

public record HockeyVideoReviewRulesInputDto(
    bool Enabled,
    bool CoachChallengeAllowed,
    bool ReviewGoals,
    bool ReviewOffsideBeforeGoal,
    bool ReviewGoalieInterference,
    bool ReviewHighStickGoal,
    bool ReviewPuckOverLine,
    HockeyCoachChallengeRulesInputDto? CoachChallengeRules = null);

public record HockeyCoachChallengeRulesInputDto(
    bool Enabled,
    int MaxChallengesPerTeam,
    bool LoseChallengeAfterFailed,
    bool PenaltyForFailedChallenge,
    int FailedChallengePenaltyMinutes,
    HockeyPenaltyOffence FailedChallengePenaltyOffence,
    HockeyPenaltySeverity FailedChallengePenaltySeverity,
    bool AllowChallengeInOvertime,
    bool AllowChallengeInShootout);

public record HockeyContactRulesInputDto(
    bool BodyCheckingAllowed,
    bool OpenIceHitsAllowed,
    bool FightingAllowed,
    bool AutomaticGameMisconductForFight,
    bool StrictHeadContactRule);
