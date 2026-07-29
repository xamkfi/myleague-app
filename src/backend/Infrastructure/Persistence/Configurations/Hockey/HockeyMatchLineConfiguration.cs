using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchLineConfiguration : BaseEntityConfiguration<HockeyMatchLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchLine> builder)
    {
        builder.ToTable("HockeyMatchLines");

        builder.Property(l => l.MatchTeamId).IsRequired();
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.LineNumber);
        builder.Property(l => l.LineType).IsRequired().HasConversion<string>();
        builder.Property(l => l.IsActive).IsRequired();
        builder.Property(l => l.IsLocked).IsRequired();
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasMany(l => l.Players)
            .WithOne(p => p.MatchLine)
            .HasForeignKey(p => p.MatchLineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.MatchTeamId)
            .HasDatabaseName("IX_HockeyMatchLines_MatchTeamId");
    }
}
