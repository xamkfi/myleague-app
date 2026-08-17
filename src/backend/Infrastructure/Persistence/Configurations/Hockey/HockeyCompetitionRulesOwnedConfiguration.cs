using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

internal static class HockeyCompetitionRulesOwnedConfiguration
{
    public static void ConfigureCompetitionRules<TOwner>(
        OwnedNavigationBuilder<TOwner, HockeyCompetitionRules> rules,
        string prefix)
        where TOwner : class
    {
        rules.Property(r => r.Name).HasColumnName($"{prefix}_Name").IsRequired().HasMaxLength(100);
        rules.Property(r => r.RuleBookVersion).HasColumnName($"{prefix}_RuleBookVersion").HasMaxLength(50);
        rules.Property(r => r.RuleBookSource).HasColumnName($"{prefix}_RuleBookSource").HasConversion<string>();

        rules.OwnsOne(r => r.MatchRules, mr =>
        {
            mr.Property(x => x.RegularPeriodCount).HasColumnName($"{prefix}_Match_RegularPeriodCount");
            mr.Property(x => x.RegularPeriodLengthMinutes).HasColumnName($"{prefix}_Match_RegularPeriodLengthMinutes");
            mr.Property(x => x.OvertimeLengthMinutes).HasColumnName($"{prefix}_Match_OvertimeLengthMinutes");
            mr.Property(x => x.StopClock).HasColumnName($"{prefix}_Match_StopClock");
            mr.Property(x => x.OvertimeEnabled).HasColumnName($"{prefix}_Match_OvertimeEnabled");
            mr.Property(x => x.ShootoutEnabled).HasColumnName($"{prefix}_Match_ShootoutEnabled");
            mr.Property(x => x.OffsideEnabled).HasColumnName($"{prefix}_Match_OffsideEnabled");
            mr.Property(x => x.DelayedOffsideEnabled).HasColumnName($"{prefix}_Match_DelayedOffsideEnabled");
            mr.Property(x => x.IcingRule).HasColumnName($"{prefix}_Match_IcingRule").HasConversion<string>();
            mr.Property(x => x.PenaltyShotEnabled).HasColumnName($"{prefix}_Match_PenaltyShotEnabled");
            mr.Property(x => x.GoaliePullAllowed).HasColumnName($"{prefix}_Match_GoaliePullAllowed");
        });

        rules.OwnsOne(r => r.StandingRules, sr =>
        {
            sr.Property(x => x.RegulationWinPoints).HasColumnName($"{prefix}_Standing_RegulationWinPoints");
            sr.Property(x => x.OvertimeWinPoints).HasColumnName($"{prefix}_Standing_OvertimeWinPoints");
            sr.Property(x => x.ShootoutWinPoints).HasColumnName($"{prefix}_Standing_ShootoutWinPoints");
            sr.Property(x => x.OvertimeLossPoints).HasColumnName($"{prefix}_Standing_OvertimeLossPoints");
            sr.Property(x => x.ShootoutLossPoints).HasColumnName($"{prefix}_Standing_ShootoutLossPoints");
            sr.Property(x => x.TiePoints).HasColumnName($"{prefix}_Standing_TiePoints");
            sr.Property<List<HockeyTieBreakerRule>>("_tieBreakers")
                .HasColumnName($"{prefix}_Standing_TieBreakers")
                .HasConversion(
                    v => string.Join(',', v.Select(x => ((int)x).ToString())),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<HockeyTieBreakerRule>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => (HockeyTieBreakerRule)int.Parse(s))
                            .ToList());
        });

        rules.OwnsOne(r => r.RosterRules, rr =>
        {
            rr.Property(x => x.MaxDressedPlayers).HasColumnName($"{prefix}_Roster_MaxDressedPlayers");
            rr.Property(x => x.MaxDressedGoalies).HasColumnName($"{prefix}_Roster_MaxDressedGoalies");
            rr.Property(x => x.MinDressedPlayers).HasColumnName($"{prefix}_Roster_MinDressedPlayers");
            rr.Property(x => x.RequiresGoalie).HasColumnName($"{prefix}_Roster_RequiresGoalie");
            rr.Property(x => x.MaxCaptains).HasColumnName($"{prefix}_Roster_MaxCaptains");
            rr.Property(x => x.MaxAlternateCaptains).HasColumnName($"{prefix}_Roster_MaxAlternateCaptains");
            rr.Property(x => x.CanGoalieBeCaptain).HasColumnName($"{prefix}_Roster_CanGoalieBeCaptain");
            rr.Property(x => x.AllowGuestPlayers).HasColumnName($"{prefix}_Roster_AllowGuestPlayers");
            rr.Property(x => x.LineManagementEnabled).HasColumnName($"{prefix}_Roster_LineManagementEnabled");
        });

        rules.OwnsOne(r => r.VideoReviewRules, vr =>
        {
            vr.Property(x => x.Enabled).HasColumnName($"{prefix}_Video_Enabled");
            vr.Property(x => x.CoachChallengeAllowed).HasColumnName($"{prefix}_Video_CoachChallengeAllowed");
            vr.Property(x => x.ReviewGoals).HasColumnName($"{prefix}_Video_ReviewGoals");
            vr.Property(x => x.ReviewOffsideBeforeGoal).HasColumnName($"{prefix}_Video_ReviewOffsideBeforeGoal");
            vr.Property(x => x.ReviewGoalieInterference).HasColumnName($"{prefix}_Video_ReviewGoalieInterference");
            vr.Property(x => x.ReviewHighStickGoal).HasColumnName($"{prefix}_Video_ReviewHighStickGoal");
            vr.Property(x => x.ReviewPuckOverLine).HasColumnName($"{prefix}_Video_ReviewPuckOverLine");
            vr.OwnsOne(x => x.CoachChallengeRules, cr =>
            {
                cr.Property(x => x.Enabled).HasColumnName($"{prefix}_Video_Challenge_Enabled");
                cr.Property(x => x.MaxChallengesPerTeam).HasColumnName($"{prefix}_Video_Challenge_MaxChallengesPerTeam");
                cr.Property(x => x.LoseChallengeAfterFailed).HasColumnName($"{prefix}_Video_Challenge_LoseChallengeAfterFailed");
                cr.Property(x => x.PenaltyForFailedChallenge).HasColumnName($"{prefix}_Video_Challenge_PenaltyForFailedChallenge");
                cr.Property(x => x.FailedChallengePenaltyMinutes).HasColumnName($"{prefix}_Video_Challenge_FailedChallengePenaltyMinutes");
                cr.Property(x => x.FailedChallengePenaltyOffence).HasColumnName($"{prefix}_Video_Challenge_FailedChallengePenaltyOffence").HasConversion<string>();
                cr.Property(x => x.FailedChallengePenaltySeverity).HasColumnName($"{prefix}_Video_Challenge_FailedChallengePenaltySeverity").HasConversion<string>();
                cr.Property(x => x.AllowChallengeInOvertime).HasColumnName($"{prefix}_Video_Challenge_AllowChallengeInOvertime");
                cr.Property(x => x.AllowChallengeInShootout).HasColumnName($"{prefix}_Video_Challenge_AllowChallengeInShootout");
            });
        });

        rules.OwnsOne(r => r.ContactRules, cr =>
        {
            cr.Property(x => x.BodyCheckingAllowed).HasColumnName($"{prefix}_Contact_BodyCheckingAllowed");
            cr.Property(x => x.OpenIceHitsAllowed).HasColumnName($"{prefix}_Contact_OpenIceHitsAllowed");
            cr.Property(x => x.FightingAllowed).HasColumnName($"{prefix}_Contact_FightingAllowed");
            cr.Property(x => x.AutomaticGameMisconductForFight).HasColumnName($"{prefix}_Contact_AutomaticGameMisconductForFight");
            cr.Property(x => x.StrictHeadContactRule).HasColumnName($"{prefix}_Contact_StrictHeadContactRule");
        });
    }
}
