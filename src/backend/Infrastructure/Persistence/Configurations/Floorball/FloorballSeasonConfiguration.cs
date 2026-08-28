using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

public class FloorballSeasonConfiguration : IEntityTypeConfiguration<FloorballSeason>
{
    public void Configure(EntityTypeBuilder<FloorballSeason> builder)
    {
        builder.HasMany(season => season.ContentBlocks)
            .WithOne()
            .HasForeignKey(block => block.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(season => season.ContentBlocks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
