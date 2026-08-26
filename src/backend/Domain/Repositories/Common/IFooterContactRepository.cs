using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository contract for footer contact entries.
/// </summary>
public interface IFooterContactRepository
{
    Task<FooterContact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FooterContact>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(FooterContact contact, CancellationToken cancellationToken = default);

    Task UpdateAsync(FooterContact contact, CancellationToken cancellationToken = default);

    Task RemoveAsync(FooterContact contact, CancellationToken cancellationToken = default);
}
