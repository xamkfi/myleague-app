using Domain.Entities.Floorball.Tournament;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTournamentGroup entity.
    /// </summary>
    public class FloorballTournamentGroupConfiguration : IEntityTypeConfiguration<FloorballTournamentGroup>
    {
        public void Configure(EntityTypeBuilder<FloorballTournamentGroup> builder)
        {
            builder.ToTable("FloorballTournamentGroups", "floorball");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.TournamentId)
                .IsRequired();

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Phase)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(g => g.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Relationship to Tournament is configured in FloorballTournamentConfiguration

            // One-to-many: Group -> GroupTeams
            builder.HasMany(g => g.Teams)
                .WithOne(gt => gt.Group)
                .HasForeignKey(gt => gt.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(g => g.Teams)
                .HasField("_teams");

            // Unique: no duplicate group names within the same tournament + phase
            builder.HasIndex(g => new { g.TournamentId, g.Name, g.Phase })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTournamentGroups_Tournament_Name_Phase");
        }
    }
}
