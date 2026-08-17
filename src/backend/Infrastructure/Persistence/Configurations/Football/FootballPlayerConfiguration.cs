using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballPlayerConfiguration : IEntityTypeConfiguration<FootballPlayer>
{
    public void Configure(EntityTypeBuilder<FootballPlayer> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PersonId).IsRequired();
        builder.Ignore(p => p.Person);
        builder.Property(p => p.IsActive).IsRequired();
        builder.OwnsOne(p => p.Position, positionBuilder =>
        {
            positionBuilder.Property(p => p.PrimaryPosition).IsRequired().HasConversion<string>();
            positionBuilder.Property(p => p.SecondaryPosition).HasConversion<string>();
        });
        builder.Property(p => p.CareerGoals).IsRequired();
        builder.Property(p => p.CareerAssists).IsRequired();
    }
}
