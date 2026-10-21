using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository contract for home page advertisements.
/// </summary>
public interface IAdvertisementRepository
{
    Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every advertisement ordered by slot and sort order.
    /// </summary>
    Task<IReadOnlyList<Advertisement>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns active advertisements whose schedule includes <paramref name="utcNow"/>, ordered by slot and sort order.
    /// </summary>
    Task<IReadOnlyList<Advertisement>> GetVisibleAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task AddAsync(Advertisement advertisement, CancellationToken cancellationToken = default);

    Task UpdateAsync(Advertisement advertisement, CancellationToken cancellationToken = default);

    Task RemoveAsync(Advertisement advertisement, CancellationToken cancellationToken = default);
}
