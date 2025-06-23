namespace Domain.Entities;

/// <summary>
/// Base class for all entities that provides common audit fields
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the UTC timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the UTC timestamp of the last update to the entity
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Protected constructor for EF Core and inheritance
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Protected constructor with existing ID (for testing or specific scenarios)
    /// </summary>
    /// <param name="id">The existing ID for the entity</param>
    protected BaseEntity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Internal method to update the UpdatedAt timestamp (called by EF Core interceptor)
    /// </summary>
    public void SetUpdatedAt(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
} 
