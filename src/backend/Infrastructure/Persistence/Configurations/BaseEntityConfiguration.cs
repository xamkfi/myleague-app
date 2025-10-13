using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations;

/// <summary>
/// Base configuration class for entities that inherit from BaseEntity
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from BaseEntity</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Configures the entity mapping
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Configure primary key
        builder.HasKey(e => e.Id);

        // Configure audit fields
        ConfigureAuditFields(builder);

        // Configure entity-specific properties
        ConfigureEntity(builder);

        // Configure audit indexes
        ConfigureAuditIndexes(builder);
    }

    /// <summary>
    /// Configures audit fields (Id, CreatedAt, UpdatedAt)
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    protected virtual void ConfigureAuditFields(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever() // Generated in constructor, not by database
            .HasComment("Unique identifier for the entity");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasComment("UTC timestamp when the entity was created");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasComment("UTC timestamp when the entity was last updated");
    }

    /// <summary>
    /// Configures audit-related indexes
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    protected virtual void ConfigureAuditIndexes(EntityTypeBuilder<TEntity> builder)
    {
        string entityName = typeof(TEntity).Name;

        // Index on CreatedAt for chronological queries
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName($"IX_{entityName}_CreatedAt");

        // Index on UpdatedAt for finding recently modified entities
        builder.HasIndex(e => e.UpdatedAt)
            .HasDatabaseName($"IX_{entityName}_UpdatedAt")
            .HasFilter("\"UpdatedAt\" IS NOT NULL");

        // Composite index for audit queries
        builder.HasIndex(e => new { e.CreatedAt, e.UpdatedAt })
            .HasDatabaseName($"IX_{entityName}_Audit")
            .IsDescending(true, true); // Most recent first
    }

    /// <summary>
    /// Override this method to configure entity-specific properties
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
} 
