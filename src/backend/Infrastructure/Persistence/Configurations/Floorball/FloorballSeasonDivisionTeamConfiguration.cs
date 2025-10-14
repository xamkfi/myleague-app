using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballSeasonDivisionTeam
    /// </summary>
    public class FloorballSeasonDivisionTeamConfiguration : IEntityTypeConfiguration<FloorballSeasonDivisionTeam>
    {
        public void Configure(EntityTypeBuilder<FloorballSeasonDivisionTeam> builder)
        {
            builder.ToTable("FloorballSeasonDivisionTeams", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SeasonDivisionId)
                .IsRequired();

            builder.Property(x => x.TeamId)
                .IsRequired();

            builder.Property(x => x.SeasonId)
                .IsRequired();

            builder.HasOne(x => x.SeasonDivision)
                .WithMany(sd => sd.Teams)
                .HasForeignKey(x => x.SeasonDivisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.SeasonDivisionId, x.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisionTeams_SeasonDivision_Team");

            // Enforce one division per team per season by unique composite index
            builder.HasIndex(x => new { x.SeasonId, x.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisionTeams_Season_Team");
        }
    }
}


