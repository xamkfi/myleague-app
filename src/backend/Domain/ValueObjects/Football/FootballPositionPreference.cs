using Domain.Enums.Football;

namespace Domain.ValueObjects.Football;

/// <summary>
/// A football player's preferred positions.
/// </summary>
public class FootballPositionPreference : IEquatable<FootballPositionPreference>
{
    public FootballPosition PrimaryPosition { get; }
    public FootballPosition? SecondaryPosition { get; }

    public FootballPositionPreference(
        FootballPosition primaryPosition,
        FootballPosition? secondaryPosition = null)
    {
        if (primaryPosition != FootballPosition.None && primaryPosition == secondaryPosition)
            throw new ArgumentException("Primary and secondary positions cannot be the same.", nameof(secondaryPosition));

        PrimaryPosition = primaryPosition;
        SecondaryPosition = secondaryPosition;
    }

    public bool CanPlayInPosition(FootballPosition position)
    {
        if (position == FootballPosition.None)
            return false;
        return position == PrimaryPosition || position == SecondaryPosition;
    }

    public bool CanPlayAsGoalkeeper =>
        PrimaryPosition == FootballPosition.Goalkeeper || SecondaryPosition == FootballPosition.Goalkeeper;

    public override bool Equals(object? obj) => Equals(obj as FootballPositionPreference);

    public bool Equals(FootballPositionPreference? other)
    {
        if (other is null)
            return false;
        return PrimaryPosition == other.PrimaryPosition && SecondaryPosition == other.SecondaryPosition;
    }

    public override int GetHashCode() => HashCode.Combine(PrimaryPosition, SecondaryPosition);

    public static bool operator ==(FootballPositionPreference? left, FootballPositionPreference? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballPositionPreference? left, FootballPositionPreference? right) => !(left == right);
}
