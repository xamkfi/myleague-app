using Domain.Entities.Floorball;
using Domain.Entities.Floorball.Tournament;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTournamentGroupTeam entity.
    /// </summary>
    public class FloorballTournamentGroupTeamConfiguration : IEntityTypeConfiguration<FloorballTournamentGroupTeam>
    {
        public void Configure(EntityTypeBuilder<FloorballTournamentGroupTeam> builder)
        {
            builder.ToTable("FloorballTournamentGroupTeams", "floorball");

            builder.HasKey(gt => gt.Id);

            builder.Property(gt => gt.GroupId)
                .IsRequired();

            builder.Property(gt => gt.TeamId)
                .IsRequired();

            builder.Property(gt => gt.TournamentId)
                .IsRequired();

            // Relationship to Group is configured in FloorballTournamentGroupConfiguration

            builder.HasOne(gt => gt.Team)
                .WithMany()
                .HasForeignKey(gt => gt.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique: a team can only be in one group within the same tournament
            builder.HasIndex(gt => new { gt.GroupId, gt.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTournamentGroupTeams_Group_Team");

            builder.HasIndex(gt => new { gt.TournamentId, gt.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTournamentGroupTeams_Tournament_Team");
        }
    }
}
