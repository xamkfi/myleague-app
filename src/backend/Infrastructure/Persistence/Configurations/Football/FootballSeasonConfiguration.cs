using Domain.Entities.Football.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballSeasonConfiguration : IEntityTypeConfiguration<FootballSeason>
{
    public void Configure(EntityTypeBuilder<FootballSeason> builder)
    {
        builder.HasMany(season => season.ContentBlocks)
            .WithOne()
            .HasForeignKey(block => block.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(season => season.ContentBlocks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
