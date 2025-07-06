using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.EventStores;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// EF Core configuration for <see cref="FloorballStoredEvent" /> entity.
/// </summary>
public class FloorballStoredEventConfiguration : IEntityTypeConfiguration<FloorballStoredEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FloorballStoredEvent> builder)
    {
        builder.ToTable("FloorballEventStore", "floorball");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AggregateId)
               .IsRequired();

        builder.Property(e => e.EventType)
               .IsRequired();

        builder.Property(e => e.Data)
               .IsRequired();

        builder.Property(e => e.Version)
               .IsRequired();

        builder.Property(e => e.OccurredOn)
               .IsRequired();

        // Ensure each (AggregateId, Version) combination is unique
        builder.HasIndex(e => new { e.AggregateId, e.Version }).IsUnique();
    }
} 
