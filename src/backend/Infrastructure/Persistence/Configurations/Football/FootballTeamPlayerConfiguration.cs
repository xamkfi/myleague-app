using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballTeamPlayerConfiguration : BaseEntityConfiguration<FootballTeamPlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FootballTeamPlayer> builder)
    {
        builder.ToTable("FootballTeamPlayers");
        builder.Property(p => p.TeamId).IsRequired();
        builder.Property(p => p.PlayerId).IsRequired();
        builder.Property(p => p.Position).IsRequired().HasConversion<string>();
        builder.Property(p => p.JerseyNumber);
        builder.Property(p => p.RequestedJerseyNumber);
        builder.Ignore(p => p.HasJerseyNumberSubstituted);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.GamesPlayed).IsRequired();
        builder.Property(p => p.Goals).IsRequired();
        builder.Property(p => p.Assists).IsRequired();
        builder.Property(p => p.YellowCards).IsRequired();
        builder.Property(p => p.RedCards).IsRequired();

        builder.HasOne<FootballTeam>()
            .WithMany(t => t.Roster)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<FootballPlayer>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(p => p.PlayerId).HasDatabaseName("IX_FootballTeamPlayer_PlayerId");
        builder.HasIndex(p => new { p.TeamId, p.PlayerId }).IsUnique().HasDatabaseName("IX_FootballTeamPlayer_TeamId_PlayerId");
        builder.HasIndex(p => new { p.TeamId, p.JerseyNumber })
            .IsUnique()
            .HasFilter("\"JerseyNumber\" IS NOT NULL")
            .HasDatabaseName("IX_FootballTeamPlayer_TeamId_JerseyNumber");
    }
}
