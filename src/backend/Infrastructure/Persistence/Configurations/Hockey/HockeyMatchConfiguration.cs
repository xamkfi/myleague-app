using Domain.Entities.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchConfiguration : BaseEntityConfiguration<HockeyMatch>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatch> builder)
    {
        builder.ToTable("HockeyMatches");

        builder.Property(m => m.CompetitionId);
        builder.Property(m => m.CompetitionDivisionId);
        builder.Property(m => m.TournamentGroupId);
        builder.Property(m => m.PlayoffSeriesId);
        builder.Property(m => m.ScheduledStartTime).IsRequired();
        builder.Property(m => m.ActualStartTime);
        builder.Property(m => m.ActualEndTime);
        builder.Property(m => m.Venue).HasMaxLength(200);
        builder.Property(m => m.MatchType).IsRequired().HasConversion<string>();
        builder.Property(m => m.Status).IsRequired().HasConversion<string>();
        builder.Property(m => m.ResultType).HasConversion<string>();
        builder.Property(m => m.CountsTowardStandings).IsRequired();
        builder.Property(m => m.CountsTowardPlayerStatistics).IsRequired();
        builder.Property(m => m.CountsTowardTeamStatistics).IsRequired();
        builder.Property(m => m.CountsTowardGoalieStatistics).IsRequired();
        builder.Property(m => m.UsesLineManagement).IsRequired();
        builder.Property(m => m.CurrentPeriodNumber).IsRequired();
        builder.Property(m => m.WentToOvertime).IsRequired();
        builder.Property(m => m.WentToShootout).IsRequired();

        builder.Ignore(m => m.HomeMatchTeam);
        builder.Ignore(m => m.AwayMatchTeam);
        builder.Ignore(m => m.HomeTeamId);
        builder.Ignore(m => m.AwayTeamId);
        builder.Ignore(m => m.HomeScore);
        builder.Ignore(m => m.AwayScore);

        builder.OwnsOne(m => m.MatchRules, ConfigureMatchRules);

        builder.HasMany(m => m.MatchTeams)
            .WithOne(t => t.Match)
            .HasForeignKey(t => t.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.MatchTeams)
            .HasField("_matchTeams")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(m => m.Officials)
            .WithOne(o => o.Match)
            .HasForeignKey(o => o.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Officials)
            .HasField("_officials")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(m => m.PeriodScores)
            .WithOne(p => p.Match)
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.PeriodScores)
            .HasField("_periodScores")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(m => m.Events)
            .HasField("_events")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(m => m.CompetitionDivision)
            .WithMany()
            .HasForeignKey(m => m.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.TournamentGroup)
            .WithMany()
            .HasForeignKey(m => m.TournamentGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.PlayoffSeries)
            .WithMany()
            .HasForeignKey(m => m.PlayoffSeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.CompetitionId)
            .HasDatabaseName("IX_HockeyMatches_CompetitionId");
        builder.HasIndex(m => m.ScheduledStartTime)
            .HasDatabaseName("IX_HockeyMatches_ScheduledStartTime");
    }

    private static void ConfigureMatchRules(OwnedNavigationBuilder<HockeyMatch, HockeyMatchRules> rules)
    {
        const string prefix = "MatchRules";
        rules.Property(x => x.RegularPeriodCount).HasColumnName($"{prefix}_RegularPeriodCount");
        rules.Property(x => x.RegularPeriodLengthMinutes).HasColumnName($"{prefix}_RegularPeriodLengthMinutes");
        rules.Property(x => x.OvertimeLengthMinutes).HasColumnName($"{prefix}_OvertimeLengthMinutes");
        rules.Property(x => x.StopClock).HasColumnName($"{prefix}_StopClock");
        rules.Property(x => x.OvertimeEnabled).HasColumnName($"{prefix}_OvertimeEnabled");
        rules.Property(x => x.ShootoutEnabled).HasColumnName($"{prefix}_ShootoutEnabled");
        rules.Property(x => x.OffsideEnabled).HasColumnName($"{prefix}_OffsideEnabled");
        rules.Property(x => x.DelayedOffsideEnabled).HasColumnName($"{prefix}_DelayedOffsideEnabled");
        rules.Property(x => x.IcingRule).HasColumnName($"{prefix}_IcingRule").HasConversion<string>();
        rules.Property(x => x.PenaltyShotEnabled).HasColumnName($"{prefix}_PenaltyShotEnabled");
        rules.Property(x => x.GoaliePullAllowed).HasColumnName($"{prefix}_GoaliePullAllowed");
    }
}
