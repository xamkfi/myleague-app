namespace WebAPI.Services;

/// <summary>
/// Centralised rate-limit windows for live match-event endpoints. Keeping the values in one
/// place makes it easier to tune the live match management UX without grepping the controller.
/// </summary>
public static class MatchEventRateLimits
{
    /// <summary>
    /// Minimum gap between consecutive goal events for the same scoring player on the same
    /// match. Tight enough to swallow accidental double-clicks but well below any realistic
    /// time between two real goals.
    /// </summary>
    public static readonly TimeSpan GoalWindow = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Minimum gap between consecutive penalty events for the same player on the same match.
    /// </summary>
    public static readonly TimeSpan PenaltyWindow = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Minimum gap between consecutive single-save submissions for the same goalie on the
    /// same match. Slightly more generous than goals because the live UI surfaces a one-tap
    /// save button which is easy to fat-finger. Bulk saves (count &gt; 1) are explicit operator
    /// actions and are not subject to this window.
    /// </summary>
    public static readonly TimeSpan SaveWindow = TimeSpan.FromMilliseconds(250);
}
