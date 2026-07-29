using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchTeamConfiguration : BaseEntityConfiguration<HockeyMatchTeam>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchTeam> builder)
    {
        builder.ToTable("HockeyMatchTeams");

        builder.Property(t => t.MatchId).IsRequired();
        builder.Property(t => t.TeamId).IsRequired();
        builder.Property(t => t.CompetitionTeamId);
        builder.Property(t => t.TeamSlot).IsRequired().HasConversion<string>();
        builder.Property(t => t.Goals).IsRequired();
        builder.Property(t => t.IsGoaliePulled).IsRequired();
        builder.Property(t => t.ActiveGoalieMatchPlayerId);
        builder.Property(t => t.TracksOnIcePlayers).IsRequired();

        builder.Ignore(t => t.Team);

        builder.HasOne(t => t.CompetitionTeam)
            .WithMany()
            .HasForeignKey(t => t.CompetitionTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.PlayerSelection)
            .WithOne(s => s.MatchTeam)
            .HasForeignKey<HockeyMatchPlayerSelection>(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Lines)
            .WithOne(l => l.MatchTeam)
            .HasForeignKey(l => l.MatchTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.OnIceState)
            .WithOne(s => s.MatchTeam)
            .HasForeignKey<HockeyOnIceState>(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ActiveGoalie)
            .WithMany()
            .HasForeignKey(t => t.ActiveGoalieMatchPlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.MatchId, t.TeamSlot })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchTeams_Match_Slot");

        builder.HasIndex(t => t.TeamId)
            .HasDatabaseName("IX_HockeyMatchTeams_TeamId");
    }
}
