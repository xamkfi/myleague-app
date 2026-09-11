using Domain.Entities.Common;
using Domain.Enums.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository interface for RulesSection entities
/// </summary>
public interface IRulesSectionRepository
{
    /// <summary>
    /// Gets a rules section by ID for read/write operations
    /// </summary>
    /// <param name="id">The section ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The section if found, null otherwise</returns>
    Task<RulesSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rules sections ordered by sort order and title
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All rules sections</returns>
    Task<IReadOnlyList<RulesSection>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a rules section exists for the given section type
    /// </summary>
    /// <param name="sectionType">The section type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a section with the type exists</returns>
    Task<bool> ExistsBySectionTypeAsync(
        RulesSectionType sectionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a rules section has child sections
    /// </summary>
    /// <param name="parentId">The parent section ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if child sections exist</returns>
    Task<bool> HasChildSectionsAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new rules section
    /// </summary>
    /// <param name="section">The section to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(RulesSection section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing rules section
    /// </summary>
    /// <param name="section">The section to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(RulesSection section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a rules section
    /// </summary>
    /// <param name="section">The section to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(RulesSection section, CancellationToken cancellationToken = default);
}
