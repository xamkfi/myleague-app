using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyPlayerConfiguration : BaseEntityConfiguration<HockeyPlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyPlayer> builder)
    {
        builder.ToTable("HockeyPlayers");

        builder.Property(p => p.PersonId).IsRequired();
        builder.Property(p => p.LicenseNumber).HasMaxLength(50);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.PrimaryPosition).IsRequired().HasConversion<string>();
        builder.Property(p => p.Shoots).IsRequired().HasConversion<string>();
        builder.Property(p => p.Catches).HasConversion<string>();

        builder.Ignore(p => p.Person);
        builder.Ignore(p => p.CareerFaceoffPercentage);

        builder.HasIndex(p => p.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_HockeyPlayers_PersonId");
    }
}
