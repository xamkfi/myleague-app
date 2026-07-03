namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Roster composition rules for a hockey competition.
/// </summary>
public class HockeyRosterRules : IEquatable<HockeyRosterRules>
{
    public int MaxDressedPlayers { get; private set; }
    public int MaxDressedGoalies { get; private set; }
    public int MinDressedPlayers { get; private set; }
    public bool RequiresGoalie { get; private set; }
    public int MaxCaptains { get; private set; }
    public int MaxAlternateCaptains { get; private set; }
    public bool CanGoalieBeCaptain { get; private set; }
    public bool AllowGuestPlayers { get; private set; }
    public bool LineManagementEnabled { get; private set; }

    private HockeyRosterRules() { }

    public HockeyRosterRules(
        int maxDressedPlayers,
        int maxDressedGoalies,
        int minDressedPlayers,
        bool requiresGoalie,
        int maxCaptains,
        int maxAlternateCaptains,
        bool canGoalieBeCaptain,
        bool allowGuestPlayers,
        bool lineManagementEnabled)
    {
        if (maxDressedPlayers < 1)
            throw new ArgumentOutOfRangeException(nameof(maxDressedPlayers), "Max dressed players must be at least 1.");
        if (minDressedPlayers < 0)
            throw new ArgumentOutOfRangeException(nameof(minDressedPlayers), "Min dressed players cannot be negative.");
        if (maxDressedPlayers < minDressedPlayers)
            throw new ArgumentException("Max dressed players cannot be less than minimum.", nameof(maxDressedPlayers));
        if (maxDressedGoalies < 0)
            throw new ArgumentOutOfRangeException(nameof(maxDressedGoalies), "Max dressed goalies cannot be negative.");
        if (maxDressedGoalies > maxDressedPlayers)
            throw new ArgumentException("Max dressed goalies cannot exceed max dressed players.", nameof(maxDressedGoalies));
        if (requiresGoalie && maxDressedGoalies < 1)
            throw new ArgumentException("At least one goalie slot is required when a goalie is mandatory.", nameof(maxDressedGoalies));
        if (maxCaptains < 0)
            throw new ArgumentOutOfRangeException(nameof(maxCaptains), "Max captains cannot be negative.");
        if (maxAlternateCaptains < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAlternateCaptains), "Max alternate captains cannot be negative.");

        MaxDressedPlayers = maxDressedPlayers;
        MaxDressedGoalies = maxDressedGoalies;
        MinDressedPlayers = minDressedPlayers;
        RequiresGoalie = requiresGoalie;
        MaxCaptains = maxCaptains;
        MaxAlternateCaptains = maxAlternateCaptains;
        CanGoalieBeCaptain = canGoalieBeCaptain;
        AllowGuestPlayers = allowGuestPlayers;
        LineManagementEnabled = lineManagementEnabled;
    }

    public static HockeyRosterRules Default() =>
        new(20, 2, 15, requiresGoalie: true, maxCaptains: 1, maxAlternateCaptains: 2,
            canGoalieBeCaptain: false, allowGuestPlayers: false, lineManagementEnabled: true);

    public override bool Equals(object? obj) => Equals(obj as HockeyRosterRules);

    public bool Equals(HockeyRosterRules? other)
    {
        if (other is null) return false;
        return MaxDressedPlayers == other.MaxDressedPlayers
            && MaxDressedGoalies == other.MaxDressedGoalies
            && MinDressedPlayers == other.MinDressedPlayers
            && RequiresGoalie == other.RequiresGoalie
            && MaxCaptains == other.MaxCaptains
            && MaxAlternateCaptains == other.MaxAlternateCaptains
            && CanGoalieBeCaptain == other.CanGoalieBeCaptain
            && AllowGuestPlayers == other.AllowGuestPlayers
            && LineManagementEnabled == other.LineManagementEnabled;
    }

    public override int GetHashCode() =>
        HashCode.Combine(MaxDressedPlayers, MaxDressedGoalies, MinDressedPlayers, RequiresGoalie,
            MaxCaptains, MaxAlternateCaptains, CanGoalieBeCaptain, AllowGuestPlayers);

    public static bool operator ==(HockeyRosterRules? left, HockeyRosterRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyRosterRules? left, HockeyRosterRules? right) => !(left == right);
}
