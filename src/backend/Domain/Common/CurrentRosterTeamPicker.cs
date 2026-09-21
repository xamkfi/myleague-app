namespace Domain.Common;

/// <summary>
/// One roster membership used to decide which club a player currently belongs to.
/// </summary>
public readonly record struct RosterMembershipCandidate(
    Guid TeamId,
    DateTime? CompetitionEnd,
    DateTime? CompetitionStart,
    bool CompetitionIsCurrent,
    bool IsActive,
    DateTime UpdatedAt);

/// <summary>
/// Picks the team from the player's latest competition membership.
/// A currently running competition wins ties, then an active roster row.
/// </summary>
public static class CurrentRosterTeamPicker
{
    public static Guid? PickTeamId(IEnumerable<RosterMembershipCandidate> memberships)
    {
        ArgumentNullException.ThrowIfNull(memberships);
        RosterMembershipCandidate? best = null;
        foreach (RosterMembershipCandidate row in memberships)
        {
            if (best == null || Compare(row, best.Value) > 0)
            {
                best = row;
            }
        }

        return best?.TeamId;
    }

    public static int Compare(RosterMembershipCandidate left, RosterMembershipCandidate right)
    {
        int byEnd = Date(left.CompetitionEnd).CompareTo(Date(right.CompetitionEnd));
        if (byEnd != 0)
        {
            return byEnd;
        }

        int byStart = Date(left.CompetitionStart).CompareTo(Date(right.CompetitionStart));
        if (byStart != 0)
        {
            return byStart;
        }

        int byCurrent = left.CompetitionIsCurrent.CompareTo(right.CompetitionIsCurrent);
        if (byCurrent != 0)
        {
            return byCurrent;
        }

        int byActive = left.IsActive.CompareTo(right.IsActive);
        if (byActive != 0)
        {
            return byActive;
        }

        return left.UpdatedAt.CompareTo(right.UpdatedAt);
    }

    private static DateTime Date(DateTime? value) => value ?? DateTime.MinValue;
}
