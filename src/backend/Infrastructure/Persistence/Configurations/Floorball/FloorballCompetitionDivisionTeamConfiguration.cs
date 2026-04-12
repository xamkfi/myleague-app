using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballCompetitionDivisionTeam
    /// </summary>
    public class FloorballCompetitionDivisionTeamConfiguration : IEntityTypeConfiguration<FloorballCompetitionDivisionTeam>
    {
        public void Configure(EntityTypeBuilder<FloorballCompetitionDivisionTeam> builder)
        {
            builder.ToTable("FloorballSeasonDivisionTeams", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CompetitionDivisionId)
                .IsRequired();

            builder.Property(x => x.TeamId)
                .IsRequired();

            builder.Property(x => x.CompetitionId)
                .IsRequired();

            builder.HasOne(x => x.CompetitionDivision)
                .WithMany(sd => sd.Teams)
                .HasForeignKey(x => x.CompetitionDivisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CompetitionDivisionId, x.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisionTeams_SeasonDivision_Team");

            builder.HasIndex(x => new { x.CompetitionId, x.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballSeasonDivisionTeams_Season_Team");
        }
    }
}
