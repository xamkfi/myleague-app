using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing club manager (club admin) link rows
/// </summary>
public interface IClubManagerRepository
{
    /// <summary>
    /// Gets a club manager row by ID
    /// </summary>
    /// <param name="id">The club manager row ID</param>
    /// <returns>The club manager row if found, null otherwise</returns>
    Task<ClubManager?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all club manager rows for a person
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>All club manager rows for the person</returns>
    Task<IEnumerable<ClubManager>> GetAllByPersonIdAsync(Guid personId);

    /// <summary>
    /// Gets all club manager rows for a club
    /// </summary>
    /// <param name="clubId">The club ID</param>
    /// <returns>All club manager rows for the club</returns>
    Task<IEnumerable<ClubManager>> GetAllByClubIdAsync(Guid clubId);

    /// <summary>
    /// Checks whether a person is an active manager of a specific club
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <param name="clubId">The club ID</param>
    /// <returns>True if an active manager row exists for the person and club</returns>
    Task<bool> IsActiveManagerOfClubAsync(Guid personId, Guid clubId);

    /// <summary>
    /// Gets a club manager row for a specific person and club
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <param name="clubId">The club ID</param>
    /// <returns>The club manager row if found, null otherwise</returns>
    Task<ClubManager?> GetByPersonAndClubAsync(Guid personId, Guid clubId);

    /// <summary>
    /// Adds a new club manager row
    /// </summary>
    /// <param name="clubManager">The club manager row to add</param>
    Task AddAsync(ClubManager clubManager);

    /// <summary>
    /// Updates an existing club manager row
    /// </summary>
    /// <param name="clubManager">The club manager row to update</param>
    Task UpdateAsync(ClubManager clubManager);
}
