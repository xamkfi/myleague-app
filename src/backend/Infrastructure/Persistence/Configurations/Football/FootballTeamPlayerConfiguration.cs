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
        builder.Property(p => p.CompetitionId);
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
        builder.HasIndex(p => new { p.TeamId, p.PlayerId, p.CompetitionId })
            .IsUnique()
            .HasFilter("\"CompetitionId\" IS NOT NULL")
            .HasDatabaseName("IX_FootballTeamPlayer_TeamId_PlayerId_CompetitionId");
        builder.HasIndex(p => new { p.TeamId, p.PlayerId })
            .IsUnique()
            .HasFilter("\"CompetitionId\" IS NULL")
            .HasDatabaseName("IX_FootballTeamPlayer_TeamId_PlayerId_Base");
        builder.HasIndex(p => new { p.TeamId, p.CompetitionId, p.JerseyNumber })
            .IsUnique()
            .HasFilter("\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NOT NULL")
            .HasDatabaseName("IX_FootballTeamPlayer_Team_Competition_Jersey");
        builder.HasIndex(p => new { p.TeamId, p.JerseyNumber })
            .IsUnique()
            .HasFilter("\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NULL")
            .HasDatabaseName("IX_FootballTeamPlayer_Team_Base_Jersey");
    }
}
