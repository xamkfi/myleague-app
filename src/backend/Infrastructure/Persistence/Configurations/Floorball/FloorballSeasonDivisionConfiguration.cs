using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballSeasonDivision
    /// </summary>
    public class FloorballSeasonDivisionConfiguration : IEntityTypeConfiguration<FloorballSeasonDivision>
    {
        public void Configure(EntityTypeBuilder<FloorballSeasonDivision> builder)
        {
            builder.ToTable("FloorballSeasonDivisions", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SeasonId)
                .IsRequired();

            builder.Property(x => x.DivisionId)
                .IsRequired();

            builder.HasOne(x => x.Season)
                .WithMany()
                .HasForeignKey(x => x.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.SeasonId, x.DivisionId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisions_Season_Division");
        }
    }
}


