using Domain.Entities.Common;

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
    /// Gets a user by username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByUsernameAsync(string username);
    
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
    /// Checks if a user with the given username exists
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>True if a user with the username exists, false otherwise</returns>
    Task<bool> ExistsByUsernameAsync(string username);
    
    /// <summary>
    /// Checks if a user with the given person ID exists
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>True if a user with the person ID exists, false otherwise</returns>
    Task<bool> ExistsByPersonIdAsync(Guid personId);
} 