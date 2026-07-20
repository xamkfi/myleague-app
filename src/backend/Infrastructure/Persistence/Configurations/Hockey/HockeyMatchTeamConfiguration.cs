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

        builder.HasIndex(t => new { t.MatchId, t.TeamSlot })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchTeams_Match_Slot");

        builder.HasIndex(t => t.TeamId)
            .HasDatabaseName("IX_HockeyMatchTeams_TeamId");
    }
}
