using Application.Features.Hockey.Competitions.DTOs;
using Domain.ValueObjects.Hockey.Rules;

namespace Application.Features.Hockey.Competitions.Mappings;

/// <summary>
/// Maps nested hockey competition rule DTOs to Domain value objects.
/// </summary>
public static class HockeyCompetitionRulesFactory
{
    public static HockeyCompetitionRules FromDto(HockeyCompetitionRulesInputDto input)
    {
        return new HockeyCompetitionRules(
            input.Name,
            input.RuleBookVersion,
            input.RuleBookSource,
            input.MatchRules is null ? HockeyMatchRules.Default() : ToMatchRules(input.MatchRules),
            input.StandingRules is null ? HockeyStandingRules.Default() : ToStandingRules(input.StandingRules),
            input.RosterRules is null ? HockeyRosterRules.Default() : ToRosterRules(input.RosterRules),
            input.VideoReviewRules is null ? HockeyVideoReviewRules.Disabled() : ToVideoReviewRules(input.VideoReviewRules),
            input.ContactRules is null ? HockeyContactRules.Default() : ToContactRules(input.ContactRules));
    }

    public static HockeyMatchRules ToMatchRules(HockeyMatchRulesInputDto dto) =>
        new(
            dto.RegularPeriodCount,
            dto.RegularPeriodLengthMinutes,
            dto.OvertimeLengthMinutes,
            dto.StopClock,
            dto.OvertimeEnabled,
            dto.ShootoutEnabled,
            dto.OffsideEnabled,
            dto.DelayedOffsideEnabled,
            dto.IcingRule,
            dto.PenaltyShotEnabled,
            dto.GoaliePullAllowed);

    public static HockeyStandingRules ToStandingRules(HockeyStandingRulesInputDto dto) =>
        new(
            dto.RegulationWinPoints,
            dto.OvertimeWinPoints,
            dto.ShootoutWinPoints,
            dto.OvertimeLossPoints,
            dto.ShootoutLossPoints,
            dto.TiePoints,
            dto.TieBreakers);

    public static HockeyRosterRules ToRosterRules(HockeyRosterRulesInputDto dto) =>
        new(
            dto.MaxDressedPlayers,
            dto.MaxDressedGoalies,
            dto.MinDressedPlayers,
            dto.RequiresGoalie,
            dto.MaxCaptains,
            dto.MaxAlternateCaptains,
            dto.CanGoalieBeCaptain,
            dto.AllowGuestPlayers,
            dto.LineManagementEnabled);

    public static HockeyVideoReviewRules ToVideoReviewRules(HockeyVideoReviewRulesInputDto dto) =>
        new(
            dto.Enabled,
            dto.CoachChallengeAllowed,
            dto.ReviewGoals,
            dto.ReviewOffsideBeforeGoal,
            dto.ReviewGoalieInterference,
            dto.ReviewHighStickGoal,
            dto.ReviewPuckOverLine,
            dto.CoachChallengeRules is null
                ? HockeyCoachChallengeRules.Disabled()
                : ToCoachChallengeRules(dto.CoachChallengeRules));

    public static HockeyCoachChallengeRules ToCoachChallengeRules(HockeyCoachChallengeRulesInputDto dto) =>
        new(
            dto.Enabled,
            dto.MaxChallengesPerTeam,
            dto.LoseChallengeAfterFailed,
            dto.PenaltyForFailedChallenge,
            dto.FailedChallengePenaltyMinutes,
            dto.FailedChallengePenaltyOffence,
            dto.FailedChallengePenaltySeverity,
            dto.AllowChallengeInOvertime,
            dto.AllowChallengeInShootout);

    public static HockeyContactRules ToContactRules(HockeyContactRulesInputDto dto) =>
        new(
            dto.BodyCheckingAllowed,
            dto.OpenIceHitsAllowed,
            dto.FightingAllowed,
            dto.AutomaticGameMisconductForFight,
            dto.StrictHeadContactRule);
}
