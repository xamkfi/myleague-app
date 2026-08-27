using Domain.Entities.Common;
using Domain.Enums.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository interface for SeasonContentBlock entities
/// </summary>
public interface ISeasonContentBlockRepository
{
    /// <summary>
    /// Gets a content block by ID
    /// </summary>
    Task<SeasonContentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content blocks for a competition ordered by sort order
    /// </summary>
    Task<IReadOnlyList<SeasonContentBlock>> GetByCompetitionIdAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content blocks for a sport and season year ordered by sort order
    /// </summary>
    Task<IReadOnlyList<SeasonContentBlock>> GetBySportAndSeasonYearAsync(
        SportsCategory sport,
        string seasonYear,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new content block
    /// </summary>
    Task AddAsync(SeasonContentBlock block, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing content block
    /// </summary>
    Task UpdateAsync(SeasonContentBlock block, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a content block
    /// </summary>
    Task RemoveAsync(SeasonContentBlock block, CancellationToken cancellationToken = default);
}
