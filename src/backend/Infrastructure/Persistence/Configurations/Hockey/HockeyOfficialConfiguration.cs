using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyOfficialConfiguration : BaseEntityConfiguration<HockeyOfficial>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyOfficial> builder)
    {
        builder.ToTable("HockeyOfficials");

        builder.Property(o => o.PersonId).IsRequired();
        builder.Property(o => o.OfficialNumber).HasMaxLength(50);
        builder.Property(o => o.OfficialRole).IsRequired().HasConversion<string>();
        builder.Property(o => o.IsActive).IsRequired();
        builder.Property(o => o.MatchesOfficiated).IsRequired();

        builder.Ignore(o => o.Person);

        builder.HasIndex(o => o.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_HockeyOfficials_PersonId");
    }
}
