using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Repository implementation for managing users
/// </summary>
public class UserRepository : RepositoryBase<User, CommonDbContext>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(CommonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found, null otherwise</returns>
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Gets a user by person ID
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByPersonIdAsync(Guid personId)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.PersonId == personId);
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>A collection of all users</returns>
    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _entities
            .Include(u => u.Person)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new user
    /// </summary>
    /// <param name="user">The user to add</param>
    public override async Task AddAsync(User user)
    {
        await _entities.AddAsync(user);
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    public override async Task UpdateAsync(User user)
    {
        _entities.Update(user);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    public async Task DeleteAsync(Guid id)
    {
        var user = await _entities.FindAsync(id);
        if (user != null)
        {
            _entities.Remove(user);
        }
    }

    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>True if the user exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _entities.AnyAsync(u => u.Id == id);
    }

    /// <summary>
    /// Checks if a user with the given username exists
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>True if a user with the username exists, false otherwise</returns>
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _entities.AnyAsync(u => u.Username == username);
    }

    /// <summary>
    /// Checks if a user with the given person ID exists
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>True if a user with the person ID exists, false otherwise</returns>
    public async Task<bool> ExistsByPersonIdAsync(Guid personId)
    {
        return await _entities.AnyAsync(u => u.PersonId == personId);
    }
} 