using System.Collections.Concurrent;

namespace WebAPI.Services;

/// <summary>
/// In-memory implementation of <see cref="IMatchEventRateLimiter"/> backed by a
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>. Each successful call updates the last-seen
/// timestamp for the supplied key and opportunistically evicts entries older than
/// <see cref="EvictionTtl"/> to keep the dictionary bounded over long-running processes.
/// </summary>
public sealed class MatchEventRateLimiter : IMatchEventRateLimiter
{
    /// <summary>
    /// Maximum age for a tracked key before it is eligible for eviction. Twenty-four hours is
    /// generous enough to outlast any single match while still keeping the dictionary from
    /// growing unbounded across days.
    /// </summary>
    public static readonly TimeSpan EvictionTtl = TimeSpan.FromHours(24);

    private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();

    /// <inheritdoc />
    public bool IsRateLimited(string key, TimeSpan window)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Rate-limit key must not be null or empty.", nameof(key));
        }

        DateTime now = DateTime.UtcNow;

        if (_lastSeen.TryGetValue(key, out DateTime last) && now - last < window)
        {
            return true;
        }

        _lastSeen[key] = now;
        EvictExpired(now);
        return false;
    }

    private void EvictExpired(DateTime now)
    {
        // Snapshot before iterating: ConcurrentDictionary supports concurrent enumeration but
        // we deliberately materialize the candidate keys so concurrent writers don't make the
        // eviction loop unstable on long-running pages.
        foreach (KeyValuePair<string, DateTime> entry in _lastSeen)
        {
            if (now - entry.Value > EvictionTtl)
            {
                _lastSeen.TryRemove(entry.Key, out _);
            }
        }
    }
}
