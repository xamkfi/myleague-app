using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for the FloorballSave entity.
/// </summary>
public class FloorballSaveConfiguration : IEntityTypeConfiguration<FloorballSave>
{
    /// <summary>
    /// Configures the entity mapping for FloorballSave.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<FloorballSave> builder)
    {
        builder.Property(s => s.GoalieId).IsRequired();
        builder.Property(s => s.WasInOvertime).IsRequired();
        builder.Property(s => s.WasInShootout).IsRequired();

        builder.HasIndex(s => s.GoalieId)
            .HasDatabaseName("IX_FloorballMatchEvent_GoalieId");
    }
}
