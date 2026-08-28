using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchActivePlayerConfiguration : BaseEntityConfiguration<HockeyMatchActivePlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchActivePlayer> builder)
    {
        builder.ToTable("HockeyMatchActivePlayers");

        builder.Property(p => p.MatchPlayerSelectionId).IsRequired();
        builder.Property(p => p.TeamPlayerId).IsRequired();
        builder.Property(p => p.JerseyNumber).IsRequired();
        builder.Property(p => p.Position).IsRequired().HasConversion<string>();
        builder.Property(p => p.CaptainRole).IsRequired().HasConversion<string>();
        builder.Property(p => p.IsStartingPlayer).IsRequired();
        builder.Property(p => p.IsGoalie).IsRequired();
        builder.Property(p => p.IsEmergencyGoalie).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.Ignore(p => p.TeamPlayer);

        builder.HasOne<HockeyTeamPlayer>()
            .WithMany()
            .HasForeignKey(p => p.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.MatchPlayerSelectionId, p.TeamPlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchActivePlayers_Selection_TeamPlayer");
    }
}
