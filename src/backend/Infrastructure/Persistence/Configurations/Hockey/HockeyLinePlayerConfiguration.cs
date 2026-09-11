using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyLinePlayerConfiguration : BaseEntityConfiguration<HockeyLinePlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyLinePlayer> builder)
    {
        builder.ToTable("HockeyLinePlayers");

        builder.Property(lp => lp.LineId).IsRequired();
        builder.Property(lp => lp.TeamPlayerId).IsRequired();
        builder.Property(lp => lp.Slot).IsRequired().HasConversion<string>();
        builder.Property(lp => lp.Order).IsRequired();

        builder.Ignore(lp => lp.TeamPlayer);

        builder.HasOne<HockeyTeamPlayer>()
            .WithMany()
            .HasForeignKey(lp => lp.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lp => new { lp.LineId, lp.TeamPlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyLinePlayers_Line_TeamPlayer");
    }
}
