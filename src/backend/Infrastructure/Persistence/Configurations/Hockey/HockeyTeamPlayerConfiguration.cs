using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTeamPlayerConfiguration : BaseEntityConfiguration<HockeyTeamPlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyTeamPlayer> builder)
    {
        builder.ToTable("HockeyTeamPlayers");

        builder.Property(p => p.TeamId).IsRequired();
        builder.Property(p => p.PlayerId).IsRequired();
        builder.Property(p => p.Position).IsRequired().HasConversion<string>();
        builder.Property(p => p.CaptainRole).IsRequired().HasConversion<string>();
        builder.Property(p => p.RosterStatus).IsRequired().HasConversion<string>();
        builder.Property(p => p.JoinedAt).IsRequired();
        builder.Property(p => p.LeftAt);

        builder.Ignore(p => p.IsActive);
        builder.Ignore(p => p.Points);
        builder.Ignore(p => p.HasJerseyNumberSubstituted);
        builder.Ignore(p => p.Player);

        builder.HasOne<HockeyPlayer>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TeamId, p.PlayerId, p.CompetitionId })
            .IsUnique()
            .HasFilter("\"LeftAt\" IS NULL")
            .HasDatabaseName("IX_HockeyTeamPlayers_Team_Player_Competition_Active");

        builder.HasIndex(p => new { p.TeamId, p.CompetitionId, p.JerseyNumber })
            .IsUnique()
            .HasFilter("\"JerseyNumber\" IS NOT NULL AND \"LeftAt\" IS NULL")
            .HasDatabaseName("IX_HockeyTeamPlayers_Team_Competition_Jersey_Active");
    }
}
