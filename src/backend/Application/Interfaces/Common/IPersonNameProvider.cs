using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    /// <summary>
    /// Provides person display names from the Common context.
    /// </summary>
    public interface IPersonNameProvider
    {
        /// <summary>
        /// Returns the person's full display name for the given personId.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Full name if found; otherwise a fallback like "Unknown".</returns>
        Task<string> GetFullNameAsync(Guid personId, CancellationToken cancellationToken = default);
    }
}


