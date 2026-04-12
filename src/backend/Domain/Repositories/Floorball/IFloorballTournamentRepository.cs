using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball tournaments
/// </summary>
public interface IFloorballTournamentRepository
{
    /// <summary>
    /// Gets a floorball tournament by ID
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The tournament if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a floorball tournament by ID with groups and group teams eagerly loaded
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The tournament with groups if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all floorball tournaments
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A list of all floorball tournaments</returns>
    Task<List<FloorballTournament>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets active floorball tournaments (not completed)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A list of active floorball tournaments</returns>
    Task<List<FloorballTournament>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to add</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(FloorballTournament tournament, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to update</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateAsync(FloorballTournament tournament, CancellationToken ct = default);

    /// <summary>
    /// Deletes a floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to delete</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteAsync(FloorballTournament tournament, CancellationToken ct = default);
}
