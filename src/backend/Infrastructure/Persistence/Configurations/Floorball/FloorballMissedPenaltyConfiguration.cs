using Domain.Entities.Floorball.Matches.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for the FloorballMissedPenalty entity.
/// </summary>
public class FloorballMissedPenaltyConfiguration : IEntityTypeConfiguration<FloorballMissedPenalty>
{
    /// <summary>
    /// Configures the entity mapping for FloorballMissedPenalty.
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballMissedPenalty> builder)
    {
        builder.Property(m => m.ShooterPlayerId).IsRequired();
    }
}
