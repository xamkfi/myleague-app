using Domain.Entities.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTournamentConfiguration : IEntityTypeConfiguration<HockeyTournament>
{
    public void Configure(EntityTypeBuilder<HockeyTournament> builder)
    {
        builder.Property(t => t.ContentHtml).HasMaxLength(50000);
        builder.Property(t => t.Venue).HasMaxLength(200);
        builder.Property(t => t.CurrentStage).IsRequired().HasConversion<string>();
        builder.Property(t => t.ChampionCompetitionTeamId);

        builder.OwnsOne(t => t.TournamentRules, tr =>
        {
            tr.Property(r => r.Format).HasColumnName("TournamentRules_Format").HasConversion<string>();
            tr.Property(r => r.HasGroupStage).HasColumnName("TournamentRules_HasGroupStage");
            tr.Property(r => r.HasPlayoffs).HasColumnName("TournamentRules_HasPlayoffs");
            tr.Property(r => r.HasBronzeGame).HasColumnName("TournamentRules_HasBronzeGame");
            tr.Property(r => r.HasPlacementGames).HasColumnName("TournamentRules_HasPlacementGames");
            tr.Property(r => r.TeamsAdvancingPerGroup).HasColumnName("TournamentRules_TeamsAdvancingPerGroup");

            tr.OwnsOne(r => r.GroupStandingRules, gsr =>
            {
                gsr.Property(x => x.RegulationWinPoints).HasColumnName("TournamentRules_GroupStanding_RegulationWinPoints");
                gsr.Property(x => x.OvertimeWinPoints).HasColumnName("TournamentRules_GroupStanding_OvertimeWinPoints");
                gsr.Property(x => x.ShootoutWinPoints).HasColumnName("TournamentRules_GroupStanding_ShootoutWinPoints");
                gsr.Property(x => x.OvertimeLossPoints).HasColumnName("TournamentRules_GroupStanding_OvertimeLossPoints");
                gsr.Property(x => x.ShootoutLossPoints).HasColumnName("TournamentRules_GroupStanding_ShootoutLossPoints");
                gsr.Property(x => x.TiePoints).HasColumnName("TournamentRules_GroupStanding_TiePoints");
            });

            tr.OwnsOne(r => r.MatchRulesOverride, mr =>
            {
                mr.Property(x => x.RegularPeriodCount).HasColumnName("TournamentRules_MatchOverride_RegularPeriodCount");
                mr.Property(x => x.RegularPeriodLengthMinutes).HasColumnName("TournamentRules_MatchOverride_RegularPeriodLengthMinutes");
                mr.Property(x => x.OvertimeLengthMinutes).HasColumnName("TournamentRules_MatchOverride_OvertimeLengthMinutes");
                mr.Property(x => x.StopClock).HasColumnName("TournamentRules_MatchOverride_StopClock");
                mr.Property(x => x.OvertimeEnabled).HasColumnName("TournamentRules_MatchOverride_OvertimeEnabled");
                mr.Property(x => x.ShootoutEnabled).HasColumnName("TournamentRules_MatchOverride_ShootoutEnabled");
                mr.Property(x => x.OffsideEnabled).HasColumnName("TournamentRules_MatchOverride_OffsideEnabled");
                mr.Property(x => x.DelayedOffsideEnabled).HasColumnName("TournamentRules_MatchOverride_DelayedOffsideEnabled");
                mr.Property(x => x.IcingRule).HasColumnName("TournamentRules_MatchOverride_IcingRule").HasConversion<string>();
                mr.Property(x => x.PenaltyShotEnabled).HasColumnName("TournamentRules_MatchOverride_PenaltyShotEnabled");
                mr.Property(x => x.GoaliePullAllowed).HasColumnName("TournamentRules_MatchOverride_GoaliePullAllowed");
            });
        });

        builder.HasMany(t => t.Groups)
            .WithOne(g => g.Tournament)
            .HasForeignKey(g => g.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
