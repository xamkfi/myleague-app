using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a new user
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// Gets the email address of the user (login identifier)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "A valid email address is required")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the person ID associated with this user
    /// </summary>
    [Required(ErrorMessage = "Person ID is required")]
    public Guid PersonId { get; init; }

    /// <summary>
    /// Gets the role of the user (defaults to ClubAdmin)
    /// </summary>
    public UserRole Role { get; init; } = UserRole.ClubAdmin;

    /// <summary>
    /// Clubs the invited club admin should manage. Only used when Role is ClubAdmin.
    /// </summary>
    public List<Guid>? ClubAssignments { get; init; }
}

/// <summary>
/// Request model for updating an existing user
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// Gets the new email address of the user
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "A valid email address is required")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role of the user
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; init; }

    /// <summary>
    /// Gets whether the user account is active
    /// </summary>
    public bool IsActive { get; init; }
}
