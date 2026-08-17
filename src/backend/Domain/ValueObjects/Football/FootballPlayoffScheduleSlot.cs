using Domain.Enums.Football;

namespace Domain.ValueObjects.Football;

/// <summary>
/// A pre-defined time/venue slot for a tournament knockout bracket.
/// </summary>
public sealed class FootballPlayoffScheduleSlot : IEquatable<FootballPlayoffScheduleSlot>
{
    public FootballPlayoffRound Round { get; private set; }
    public int Order { get; private set; }
    public DateTime ScheduledDateTime { get; private set; }
    public string? Venue { get; private set; }

    private FootballPlayoffScheduleSlot()
    {
        Round = FootballPlayoffRound.None;
    }

    public FootballPlayoffScheduleSlot(
        FootballPlayoffRound round,
        int order,
        DateTime scheduledDateTime,
        string? venue = null)
    {
        if (round == FootballPlayoffRound.None)
            throw new ArgumentException("Playoff schedule slot must specify a real round.", nameof(round));
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Playoff slot order cannot be negative.");

        Round = round;
        Order = order;
        ScheduledDateTime = scheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => scheduledDateTime,
            DateTimeKind.Local => scheduledDateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledDateTime, DateTimeKind.Utc)
        };
        Venue = string.IsNullOrWhiteSpace(venue) ? null : venue.Trim();
    }

    public override bool Equals(object? obj) => Equals(obj as FootballPlayoffScheduleSlot);

    public bool Equals(FootballPlayoffScheduleSlot? other)
    {
        if (other is null)
            return false;
        return Round == other.Round
            && Order == other.Order
            && ScheduledDateTime == other.ScheduledDateTime
            && string.Equals(Venue, other.Venue, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(Round, Order, ScheduledDateTime, Venue);
}
