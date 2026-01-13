using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a new division
/// </summary>
public record CreateDivisionRequest
{
    /// <summary>
    /// Gets the name of the division
    /// </summary>
    /// <example>First Division</example>
    [Required(ErrorMessage = "Division name is required")]
    [StringLength(100, ErrorMessage = "Division name cannot exceed 100 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of the division
    /// </summary>
    /// <example>The highest competitive level for professional teams</example>
    [Required(ErrorMessage = "Division description is required")]
    [StringLength(500, ErrorMessage = "Division description cannot exceed 500 characters")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the competitive level of the division (1 being highest)
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Division level is required")]
    [Range(1, 10, ErrorMessage = "Division level must be between 1 and 10")]
    public int Level { get; init; }

    /// <summary>
    /// Gets the sport type this division is for
    /// </summary>
    /// <example>Floorball</example>
    [Required(ErrorMessage = "Sport type is required")]
    public SportsCategory SportType { get; init; } = SportsCategory.None;
}

/// <summary>
/// Request model for updating an existing division
/// </summary>
public record UpdateDivisionRequest
{
    /// <summary>
    /// Gets the name of the division
    /// </summary>
    /// <example>First Division</example>
    [Required(ErrorMessage = "Division name is required")]
    [StringLength(100, ErrorMessage = "Division name cannot exceed 100 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of the division
    /// </summary>
    /// <example>The highest competitive level for professional teams</example>
    [Required(ErrorMessage = "Division description is required")]
    [StringLength(500, ErrorMessage = "Division description cannot exceed 500 characters")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the competitive level of the division (1 being highest)
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Division level is required")]
    [Range(1, 10, ErrorMessage = "Division level must be between 1 and 10")]
    public int Level { get; init; }
} 