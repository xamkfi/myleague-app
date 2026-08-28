using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyOnIceChangeConfiguration : BaseEntityConfiguration<HockeyOnIceChange>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyOnIceChange> builder)
    {
        builder.ToTable("HockeyOnIceChanges");

        builder.Property(c => c.OnIceStateId).IsRequired();
        builder.Property(c => c.ChangeType).IsRequired().HasConversion<string>();
        builder.Property(c => c.OutgoingActivePlayerId);
        builder.Property(c => c.IncomingActivePlayerId);
        builder.Property(c => c.AppliedLineId);
        builder.Property(c => c.PeriodNumber);
        builder.Property(c => c.GameTime);
        builder.Property(c => c.CreatedByUserId);

        builder.HasOne(c => c.OutgoingPlayer)
            .WithMany()
            .HasForeignKey(c => c.OutgoingActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.IncomingPlayer)
            .WithMany()
            .HasForeignKey(c => c.IncomingActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AppliedLine)
            .WithMany()
            .HasForeignKey(c => c.AppliedLineId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.OnIceStateId)
            .HasDatabaseName("IX_HockeyOnIceChanges_OnIceStateId");
    }
}
