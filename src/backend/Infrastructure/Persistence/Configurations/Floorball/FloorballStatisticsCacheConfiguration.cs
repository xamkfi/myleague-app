using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballStatisticsCache entity.
    /// </summary>
    public class FloorballStatisticsCacheConfiguration : IEntityTypeConfiguration<FloorballStatisticsCache>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballStatisticsCache.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballStatisticsCache> builder)
        {
            // Configure table
            builder.ToTable("FloorballStatisticsCache");
            
            // Configure primary key
            builder.HasKey(s => s.Id);

            // Configure cache key
            builder.Property(s => s.CacheKey)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Unique cache key identifier");

            // Configure foreign key
            builder.Property(s => s.SeasonId)
                .HasComment("Optional season ID this cache is associated with");

            // Ignore navigation property to prevent cross-context issues
            builder.Ignore(s => s.Season);

            // Configure JSON data
            builder.Property(s => s.JsonData)
                .IsRequired()
                .HasColumnType("text")
                .HasComment("Serialized JSON data");

            // Configure timestamps
            builder.Property(s => s.LastUpdated)
                .IsRequired()
                .HasComment("When this cache entry was last updated");

            builder.Property(s => s.ExpiresAt)
                .IsRequired()
                .HasComment("When this cache entry expires");

            // Configure indexes for performance
            builder.HasIndex(s => s.CacheKey)
                .IsUnique()
                .HasDatabaseName("IX_FloorballStatisticsCache_CacheKey");

            builder.HasIndex(s => s.SeasonId)
                .HasDatabaseName("IX_FloorballStatisticsCache_SeasonId");

            builder.HasIndex(s => s.ExpiresAt)
                .HasDatabaseName("IX_FloorballStatisticsCache_ExpiresAt");

            builder.HasIndex(s => new { s.SeasonId, s.ExpiresAt })
                .HasDatabaseName("IX_FloorballStatisticsCache_SeasonId_ExpiresAt");

            // Configure base entity properties
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasComment("UTC timestamp when the entity was created");

            builder.Property(s => s.UpdatedAt)
                .HasComment("UTC timestamp when the entity was last updated");
        }
    }
}
