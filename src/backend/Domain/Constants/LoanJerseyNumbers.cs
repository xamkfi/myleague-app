namespace Domain.Constants;

/// <summary>
/// Picks a jersey number for a loan player from the team's roster rows.
/// </summary>
public static class LoanJerseyNumbers
{
    public const int Minimum = 1;
    public const int Maximum = 99;

    public const string NoneAvailableMessage = "No free jersey number is available for a loan player.";

    /// <summary>
    /// One roster row's jersey, used when choosing a free number across the whole team.
    /// </summary>
    public readonly record struct RosterJersey(Guid PlayerId, Guid? CompetitionId, int? JerseyNumber);

    /// <summary>
    /// Keeps the player's number on this competition. Otherwise prefers a number they
    /// already wear on another roster row, then the lowest free number in 1–99 that no
    /// other player on the team uses.
    /// </summary>
    public static int Resolve(IEnumerable<RosterJersey> roster, Guid playerId, Guid? competitionId)
    {
        ArgumentNullException.ThrowIfNull(roster);
        List<RosterJersey> rows = roster.ToList();

        RosterJersey? membership = null;
        foreach (RosterJersey row in rows)
        {
            if (row.PlayerId == playerId && row.CompetitionId == competitionId)
            {
                membership = row;
                break;
            }
        }

        if (membership?.JerseyNumber is int current)
        {
            return current;
        }

        int? preferred = null;
        HashSet<int> usedByOthers = new();
        foreach (RosterJersey row in rows)
        {
            if (row.JerseyNumber is not int number)
            {
                continue;
            }

            if (row.PlayerId == playerId)
            {
                preferred ??= number;
            }
            else
            {
                usedByOthers.Add(number);
            }
        }

        int? jersey = Select(usedByOthers, preferred);
        if (jersey == null)
        {
            throw new InvalidOperationException(NoneAvailableMessage);
        }

        return jersey.Value;
    }

    /// <summary>
    /// How many numbers in 1–99 are absent from <paramref name="used"/>.
    /// </summary>
    public static int FreeCount(IEnumerable<int> used)
    {
        ArgumentNullException.ThrowIfNull(used);
        HashSet<int> taken = new();
        foreach (int number in used)
        {
            if (number >= Minimum && number <= Maximum)
            {
                taken.Add(number);
            }
        }

        return Maximum - Minimum + 1 - taken.Count;
    }

    /// <summary>
    /// <paramref name="preferred"/> when it is inside 1–99 and not used, otherwise the lowest free number.
    /// </summary>
    public static int? Select(IEnumerable<int> usedByOthers, int? preferred)
    {
        ArgumentNullException.ThrowIfNull(usedByOthers);
        HashSet<int> taken = usedByOthers as HashSet<int> ?? usedByOthers.ToHashSet();
        if (preferred is int preferredNumber
            && preferredNumber >= Minimum
            && preferredNumber <= Maximum
            && !taken.Contains(preferredNumber))
        {
            return preferredNumber;
        }

        for (int number = Minimum; number <= Maximum; number++)
        {
            if (!taken.Contains(number))
            {
                return number;
            }
        }

        return null;
    }
}
