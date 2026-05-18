using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// EF Core configuration for FloorballTournamentGroup
    /// </summary>
    public class FloorballTournamentGroupConfiguration : IEntityTypeConfiguration<FloorballTournamentGroup>
    {
        public void Configure(EntityTypeBuilder<FloorballTournamentGroup> builder)
        {
            builder.ToTable("FloorballTournamentGroups", "floorball");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.TournamentId)
                .IsRequired();

            builder.HasIndex(x => new { x.TournamentId, x.Order })
                .HasDatabaseName("IX_FloorballTournamentGroups_Tournament_Order");
        }
    }
}
