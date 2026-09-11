namespace WebAPI.Services;

/// <summary>
/// Lightweight per-process rate limiter for live match-event endpoints (goal, penalty, save).
/// Prevents accidental double-clicks from the live match management UI from creating duplicate
/// events without rejecting legitimate bulk operations.
/// </summary>
/// <remarks>
/// The default implementation backs onto an in-memory <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
/// and is therefore process-local. If the API is ever scaled out horizontally this should be
/// swapped for a distributed implementation (e.g. backed by Redis or <c>IMemoryCache</c>).
/// </remarks>
public interface IMatchEventRateLimiter
{
    /// <summary>
    /// Returns <c>true</c> when an event with the supplied <paramref name="key"/> was last seen
    /// less than <paramref name="window"/> ago, in which case the caller should reject the
    /// request. When the window has elapsed (or the key is unseen), the call records the current
    /// timestamp and returns <c>false</c>.
    /// </summary>
    /// <param name="key">Stable identifier composed of match id + event type + scoring entity.</param>
    /// <param name="window">Window during which a repeat event with the same key is rejected.</param>
    bool IsRateLimited(string key, TimeSpan window);
}
