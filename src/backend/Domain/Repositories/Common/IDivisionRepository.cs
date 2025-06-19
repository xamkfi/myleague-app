using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository interface for Division entities
/// </summary>
public interface IDivisionRepository
{
    /// <summary>
    /// Gets a division by its identifier
    /// </summary>
    /// <param name="id">The unique identifier of the division</param>
    /// <returns>The division if found, null otherwise</returns>
    Task<Division?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all divisions
    /// </summary>
    /// <returns>A collection of all divisions</returns>
    Task<IEnumerable<Division>> GetAllAsync();

    /// <summary>
    /// Gets divisions by sport type
    /// </summary>
    /// <param name="sportType">The sport type to filter by</param>
    /// <returns>A collection of divisions for the specified sport type</returns>
    Task<IEnumerable<Division>> GetBySportTypeAsync(string sportType);

    /// <summary>
    /// Gets active divisions by sport type
    /// </summary>
    /// <param name="sportType">The sport type to filter by</param>
    /// <returns>A collection of active divisions for the specified sport type</returns>
    Task<IEnumerable<Division>> GetActiveBySportTypeAsync(string sportType);

    /// <summary>
    /// Gets a division by name and sport type
    /// </summary>
    /// <param name="name">The name of the division</param>
    /// <param name="sportType">The sport type</param>
    /// <returns>The division if found, null otherwise</returns>
    Task<Division?> GetByNameAndSportTypeAsync(string name, string sportType);

    /// <summary>
    /// Adds a new division
    /// </summary>
    /// <param name="division">The division to add</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task AddAsync(Division division);

    /// <summary>
    /// Updates an existing division
    /// </summary>
    /// <param name="division">The division to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task UpdateAsync(Division division);

    /// <summary>
    /// Deletes a division
    /// </summary>
    /// <param name="division">The division to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(Division division);

    /// <summary>
    /// Checks if a division exists by name and sport type
    /// </summary>
    /// <param name="name">The name of the division</param>
    /// <param name="sportType">The sport type</param>
    /// <returns>True if the division exists, false otherwise</returns>
    Task<bool> ExistsAsync(string name, string sportType);
} 