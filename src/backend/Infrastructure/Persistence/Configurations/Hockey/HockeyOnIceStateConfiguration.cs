using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyOnIceStateConfiguration : BaseEntityConfiguration<HockeyOnIceState>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyOnIceState> builder)
    {
        builder.ToTable("HockeyOnIceStates");

        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.IsEnabled).IsRequired();
        builder.Property(s => s.LastUpdatedAt).IsRequired();
        builder.Property(s => s.LastUpdatedByUserId);
        builder.Property(s => s.Version).IsRequired();

        builder.HasMany(s => s.PlayersOnIce)
            .WithOne(p => p.OnIceState)
            .HasForeignKey(p => p.OnIceStateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.PlayersOnIce)
            .HasField("_playersOnIce")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.ChangeLog)
            .WithOne(c => c.OnIceState)
            .HasForeignKey(c => c.OnIceStateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.ChangeLog)
            .HasField("_changeLog")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.MatchTeamId)
            .IsUnique()
            .HasDatabaseName("IX_HockeyOnIceStates_MatchTeamId");
    }
}
