using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballCompetitionDivision
    /// </summary>
    public class FloorballCompetitionDivisionConfiguration : IEntityTypeConfiguration<FloorballCompetitionDivision>
    {
        public void Configure(EntityTypeBuilder<FloorballCompetitionDivision> builder)
        {
            builder.ToTable("FloorballSeasonDivisions", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CompetitionId)
                .IsRequired();

            builder.Property(x => x.DivisionId)
                .IsRequired();

            builder.HasOne(x => x.Competition)
                .WithMany()
                .HasForeignKey(x => x.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.CompetitionId, x.DivisionId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisions_Season_Division");
        }
    }
}
