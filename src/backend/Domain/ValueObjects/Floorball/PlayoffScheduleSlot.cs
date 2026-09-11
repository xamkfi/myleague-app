using Domain.Enums.Floorball;

namespace Domain.ValueObjects.Floorball;

/// <summary>
/// A single pre-defined time/venue slot for a tournament's playoff bracket, captured at
/// import time before the qualifying teams are known.
///
/// The schedule is purely a planning aid: the tournament page renders each slot as a
/// "TBD vs TBD" entry so end-users see the full match program (group + playoff) in advance.
/// When the admin advances the tournament to the playoff stage,
/// <see cref="Application.Features.Floorball.Tournaments.Handlers.StartTournamentPlayoffStageHandler"/>
/// looks up the matching slot by <see cref="Round"/> + <see cref="Order"/> and uses these
/// values for the real <see cref="Entities.Floorball.FloorballMatch"/> records it creates.
/// </summary>
public sealed class PlayoffScheduleSlot : IEquatable<PlayoffScheduleSlot>
{
    /// <summary>
    /// Bracket round this slot belongs to (QuarterFinal, SemiFinal, ThirdPlaceMatch, Final).
    /// </summary>
    public FloorballPlayoffRound Round { get; private set; }

    /// <summary>
    /// 0-based position of the match within its round. Matches the
    /// <see cref="PlayoffBracketBuilder.PlannedMatch.Order"/> emitted by the bracket builder
    /// so the StartPlayoff handler can pair slots to planned matches deterministically.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Pre-defined kickoff time (UTC). Used as-is for the generated playoff match.
    /// </summary>
    public DateTime ScheduledDateTime { get; private set; }

    /// <summary>
    /// Optional venue / court label (e.g. "Mikkeli — Kenttä 1"). Falls back to the
    /// tournament venue when omitted.
    /// </summary>
    public string? Venue { get; private set; }

    private PlayoffScheduleSlot()
    {
        // EF Core constructor — leave fields at default; values are populated via owned-entity binding.
        Round = FloorballPlayoffRound.None;
    }

    public PlayoffScheduleSlot(
        FloorballPlayoffRound round,
        int order,
        DateTime scheduledDateTime,
        string? venue = null)
    {
        if (round == FloorballPlayoffRound.None)
        {
            throw new ArgumentException("Playoff schedule slot must specify a real round (QuarterFinal, SemiFinal, ThirdPlaceMatch or Final).", nameof(round));
        }
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Playoff slot order cannot be negative.");
        }

        Round = round;
        Order = order;
        ScheduledDateTime = scheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => scheduledDateTime,
            DateTimeKind.Local => scheduledDateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(scheduledDateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(scheduledDateTime, DateTimeKind.Utc)
        };
        Venue = string.IsNullOrWhiteSpace(venue) ? null : venue.Trim();
    }

    public override bool Equals(object? obj) => Equals(obj as PlayoffScheduleSlot);

    public bool Equals(PlayoffScheduleSlot? other)
    {
        if (other is null) return false;
        return Round == other.Round
            && Order == other.Order
            && ScheduledDateTime == other.ScheduledDateTime
            && string.Equals(Venue, other.Venue, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(Round, Order, ScheduledDateTime, Venue);
}
