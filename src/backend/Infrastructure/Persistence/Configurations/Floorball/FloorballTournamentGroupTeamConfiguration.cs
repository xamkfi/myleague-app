using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballTournamentGroupTeam
    /// </summary>
    public class FloorballTournamentGroupTeamConfiguration : IEntityTypeConfiguration<FloorballTournamentGroupTeam>
    {
        public void Configure(EntityTypeBuilder<FloorballTournamentGroupTeam> builder)
        {
            builder.ToTable("FloorballTournamentGroupTeams", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TournamentGroupId)
                .IsRequired();

            builder.Property(x => x.TeamId)
                .IsRequired();

            builder.HasOne(x => x.TournamentGroup)
                .WithMany(g => g.Teams)
                .HasForeignKey(x => x.TournamentGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TournamentGroupId, x.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTournamentGroupTeams_Group_Team");
        }
    }
}
