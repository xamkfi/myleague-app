using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyOnIcePlayerConfiguration : BaseEntityConfiguration<HockeyOnIcePlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyOnIcePlayer> builder)
    {
        builder.ToTable("HockeyOnIcePlayers");

        builder.Property(p => p.OnIceStateId).IsRequired();
        builder.Property(p => p.MatchActivePlayerId).IsRequired();
        builder.Property(p => p.Slot).HasConversion<string>();
        builder.Property(p => p.Order);
        builder.Property(p => p.IsGoalie).IsRequired();
        builder.Property(p => p.IsExtraAttacker).IsRequired();
        builder.Property(p => p.AddedAt).IsRequired();

        builder.HasOne(p => p.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(p => p.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.OnIceStateId, p.MatchActivePlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyOnIcePlayers_State_ActivePlayer");
    }
}
