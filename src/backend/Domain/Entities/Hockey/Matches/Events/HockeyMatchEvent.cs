using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

/// <summary>
/// Abstract base for all hockey match events. Events are the source of truth for statistics.
/// Player references use <see cref="HockeyMatchActivePlayer"/>, never career players directly.
/// </summary>
public abstract class HockeyMatchEvent : BaseEntity
{
    public Guid MatchId { get; protected set; }
    public HockeyMatch Match { get; protected set; } = null!;

    public HockeyMatchEventType EventType { get; protected set; }

    public Guid? MatchTeamId { get; protected set; }
    public HockeyMatchTeam? MatchTeam { get; protected set; }

    public Guid? MatchActivePlayerId { get; protected set; }
    public HockeyMatchActivePlayer? MatchActivePlayer { get; protected set; }

    public int PeriodNumber { get; protected set; }
    public TimeSpan GameTime { get; protected set; }
    public string? Description { get; protected set; }

    public string FormattedGameTime
    {
        get
        {
            int totalSeconds = (int)Math.Floor(GameTime.TotalSeconds);
            if (totalSeconds < 0)
                totalSeconds = 0;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }

    protected HockeyMatchEvent() { }

    protected HockeyMatchEvent(
        Guid matchId,
        HockeyMatchEventType eventType,
        int periodNumber,
        TimeSpan gameTime,
        Guid? matchTeamId = null,
        Guid? matchActivePlayerId = null,
        string? description = null)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be at least 1.");
        if (gameTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gameTime), "Game time cannot be negative.");
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));

        MatchId = matchId;
        EventType = eventType;
        PeriodNumber = periodNumber;
        GameTime = gameTime;
        MatchTeamId = matchTeamId;
        MatchActivePlayerId = matchActivePlayerId;
        Description = description;
    }

    /// <summary>
    /// Updates shared timing and description fields used by live-ops corrections.
    /// </summary>
    protected void UpdateTiming(int periodNumber, TimeSpan gameTime, string? description)
    {
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be at least 1.");
        if (gameTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gameTime), "Game time cannot be negative.");

        PeriodNumber = periodNumber;
        GameTime = gameTime;
        Description = description;
    }

    /// <summary>
    /// Updates the primary match-team / active-player refs on the base event row.
    /// </summary>
    protected void UpdatePrimaryReferences(Guid? matchTeamId, Guid? matchActivePlayerId)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));

        MatchTeamId = matchTeamId;
        MatchActivePlayerId = matchActivePlayerId;
    }
}
