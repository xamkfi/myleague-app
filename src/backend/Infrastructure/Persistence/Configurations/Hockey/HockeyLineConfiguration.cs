using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyLineConfiguration : BaseEntityConfiguration<HockeyLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyLine> builder)
    {
        builder.ToTable("HockeyLines");

        builder.Property(l => l.TeamId).IsRequired();
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.LineNumber).IsRequired();
        builder.Property(l => l.LineType).IsRequired().HasConversion<string>();
        builder.Property(l => l.IsActive).IsRequired();

        builder.HasMany(l => l.Players)
            .WithOne(lp => lp.Line)
            .HasForeignKey(lp => lp.LineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(l => l.Players)
            .HasField("_players")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
