using Domain.Entities.Common;
using Domain.Enums.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing users
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user by person ID
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByPersonIdAsync(Guid personId);

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>A collection of all users</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Adds a new user
    /// </summary>
    /// <param name="user">The user to add</param>
    Task AddAsync(User user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    Task UpdateAsync(User user);

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>True if the user exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Checks if a user with the given email exists
    /// </summary>
    /// <param name="email">The email address</param>
    /// <returns>True if a user with the email exists, false otherwise</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Checks if a user with the given person ID exists
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>True if a user with the person ID exists, false otherwise</returns>
    Task<bool> ExistsByPersonIdAsync(Guid personId);

    /// <summary>
    /// Gets a user by their email verification token
    /// </summary>
    /// <param name="token">The email verification token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailVerificationTokenAsync(string token);

    /// <summary>
    /// Counts users with the given role.
    /// </summary>
    Task<int> CountByRoleAsync(UserRole role);
}
