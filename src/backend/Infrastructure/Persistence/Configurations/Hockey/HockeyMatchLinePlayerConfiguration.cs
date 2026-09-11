using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchLinePlayerConfiguration : BaseEntityConfiguration<HockeyMatchLinePlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchLinePlayer> builder)
    {
        builder.ToTable("HockeyMatchLinePlayers");

        builder.Property(p => p.MatchLineId).IsRequired();
        builder.Property(p => p.MatchActivePlayerId).IsRequired();
        builder.Property(p => p.Slot).HasConversion<string>();
        builder.Property(p => p.Order);

        builder.HasOne(p => p.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(p => p.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.MatchLineId, p.MatchActivePlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchLinePlayers_Line_ActivePlayer");
    }
}
