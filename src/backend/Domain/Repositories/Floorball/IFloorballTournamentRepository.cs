using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball tournaments
/// </summary>
public interface IFloorballTournamentRepository
{
    /// <summary>
    /// Gets a floorball tournament by ID with all related data
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <returns>The tournament if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a floorball tournament by ID with groups and their teams
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <returns>The tournament if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id);

    /// <summary>
    /// Gets all floorball tournaments
    /// </summary>
    /// <returns>A collection of all floorball tournaments</returns>
    Task<IEnumerable<FloorballTournament>> GetAllAsync();

    /// <summary>
    /// Gets floorball tournaments by status
    /// </summary>
    /// <param name="status">The tournament status to filter by</param>
    /// <returns>A collection of tournaments with the specified status</returns>
    Task<IEnumerable<FloorballTournament>> GetByStatusAsync(FloorballTournamentStatus status);

    /// <summary>
    /// Adds a new floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to add</param>
    Task AddAsync(FloorballTournament tournament);

    /// <summary>
    /// Updates an existing floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to update</param>
    Task UpdateAsync(FloorballTournament tournament);

    /// <summary>
    /// Deletes a floorball tournament
    /// </summary>
    /// <param name="id">The ID of the tournament to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a floorball tournament exists
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <returns>True if the tournament exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
