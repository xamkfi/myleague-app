using Domain.Enums.Hockey.Competitions;

namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Point allocation and tie-breaking rules for hockey standings.
/// </summary>
public class HockeyStandingRules : IEquatable<HockeyStandingRules>
{
    public int RegulationWinPoints { get; private set; }
    public int OvertimeWinPoints { get; private set; }
    public int ShootoutWinPoints { get; private set; }
    public int OvertimeLossPoints { get; private set; }
    public int ShootoutLossPoints { get; private set; }
    public int TiePoints { get; private set; }
    public IReadOnlyList<HockeyTieBreakerRule> TieBreakers => _tieBreakers.AsReadOnly();
    private readonly List<HockeyTieBreakerRule> _tieBreakers = new();

    private HockeyStandingRules() { }

    public HockeyStandingRules(
        int regulationWinPoints,
        int overtimeWinPoints,
        int shootoutWinPoints,
        int overtimeLossPoints,
        int shootoutLossPoints,
        int tiePoints,
        IEnumerable<HockeyTieBreakerRule>? tieBreakers = null)
    {
        ValidatePoints(regulationWinPoints, nameof(regulationWinPoints));
        ValidatePoints(overtimeWinPoints, nameof(overtimeWinPoints));
        ValidatePoints(shootoutWinPoints, nameof(shootoutWinPoints));
        ValidatePoints(overtimeLossPoints, nameof(overtimeLossPoints));
        ValidatePoints(shootoutLossPoints, nameof(shootoutLossPoints));
        ValidatePoints(tiePoints, nameof(tiePoints));

        RegulationWinPoints = regulationWinPoints;
        OvertimeWinPoints = overtimeWinPoints;
        ShootoutWinPoints = shootoutWinPoints;
        OvertimeLossPoints = overtimeLossPoints;
        ShootoutLossPoints = shootoutLossPoints;
        TiePoints = tiePoints;

        List<HockeyTieBreakerRule> materialized = tieBreakers?.ToList()
            ?? DefaultTieBreakers().ToList();
        if (materialized.Count == 0)
            throw new ArgumentException("At least one tie-breaker rule is required.", nameof(tieBreakers));
        if (materialized.Distinct().Count() != materialized.Count)
            throw new ArgumentException("Tie-breaker rules cannot contain duplicates.", nameof(tieBreakers));

        _tieBreakers = materialized;
    }

    public static HockeyStandingRules Default() =>
        new(3, 2, 2, 1, 1, 1, DefaultTieBreakers());

    private static IEnumerable<HockeyTieBreakerRule> DefaultTieBreakers() =>
    [
        HockeyTieBreakerRule.Points,
        HockeyTieBreakerRule.RegulationWins,
        HockeyTieBreakerRule.GoalDifference,
        HockeyTieBreakerRule.GoalsFor
    ];

    private static void ValidatePoints(int points, string paramName)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException(paramName, "Point values cannot be negative.");
    }

    public override bool Equals(object? obj) => Equals(obj as HockeyStandingRules);

    public bool Equals(HockeyStandingRules? other)
    {
        if (other is null) return false;
        return RegulationWinPoints == other.RegulationWinPoints
            && OvertimeWinPoints == other.OvertimeWinPoints
            && ShootoutWinPoints == other.ShootoutWinPoints
            && OvertimeLossPoints == other.OvertimeLossPoints
            && ShootoutLossPoints == other.ShootoutLossPoints
            && TiePoints == other.TiePoints
            && _tieBreakers.SequenceEqual(other._tieBreakers);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(RegulationWinPoints);
        hash.Add(OvertimeWinPoints);
        hash.Add(ShootoutWinPoints);
        hash.Add(OvertimeLossPoints);
        hash.Add(ShootoutLossPoints);
        hash.Add(TiePoints);
        foreach (HockeyTieBreakerRule rule in _tieBreakers)
            hash.Add(rule);
        return hash.ToHashCode();
    }

    public static bool operator ==(HockeyStandingRules? left, HockeyStandingRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyStandingRules? left, HockeyStandingRules? right) => !(left == right);
}
