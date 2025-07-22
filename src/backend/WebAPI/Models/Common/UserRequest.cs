using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a new user
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// Gets the username of the user
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password of the user
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the person ID associated with this user
    /// </summary>
    [Required(ErrorMessage = "Person ID is required")]
    public Guid PersonId { get; init; }
}

/// <summary>
/// Request model for updating an existing user
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// Gets the username of the user
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the new password of the user (optional)
    /// </summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string? Password { get; init; }
}

/// <summary>
/// Request model for updating user password
/// </summary>
public record UpdateUserPasswordRequest
{
    /// <summary>
    /// Gets the current password of the user
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// Gets the new password of the user
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "New password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string NewPassword { get; init; } = string.Empty;
} 