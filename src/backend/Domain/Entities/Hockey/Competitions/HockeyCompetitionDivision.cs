using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Links a <see cref="HockeyCompetition"/> to a division from the common context.
/// Primarily used by <see cref="HockeySeason"/> to partition teams into divisions.
/// Teams are assigned via <see cref="HockeyCompetitionDivisionTeam"/> memberships that
/// reference <see cref="HockeyCompetitionTeam"/>, not <c>HockeyTeam</c> directly.
/// <see cref="HockeyCompetition.AddTeamToDivision"/> orchestrates placement and ensures
/// each active competition team belongs to at most one division.
/// </summary>
public class HockeyCompetitionDivision : BaseEntity
{
    /// <summary>Gets the competition this division belongs to.</summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>Gets the parent competition aggregate.</summary>
    public HockeyCompetition Competition { get; private set; } = null!;

    /// <summary>
    /// Gets the division identifier from the common context (no cross-context navigation).
    /// </summary>
    public Guid DivisionId { get; private set; }

    /// <summary>Gets the display name of this division within the competition.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the display order among sibling divisions.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Gets whether this division link is active (soft-deactivated via <see cref="Deactivate"/>).</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets the competition team id of the division champion, if decided.</summary>
    public Guid? ChampionCompetitionTeamId { get; private set; }

    /// <summary>Gets the division champion as a competition team reference.</summary>
    public HockeyCompetitionTeam? ChampionCompetitionTeam { get; private set; }

    /// <summary>Gets optional rule overrides that apply only within this division.</summary>
    public HockeyCompetitionRules? RulesOverride { get; private set; }

    /// <summary>Gets the competition teams assigned to this division.</summary>
    public IReadOnlyCollection<HockeyCompetitionDivisionTeam> Teams => _teams.AsReadOnly();
    private readonly List<HockeyCompetitionDivisionTeam> _teams = new();

    private HockeyCompetitionDivision() { }

    internal HockeyCompetitionDivision(
        Guid competitionId,
        Guid divisionId,
        string name,
        int sortOrder,
        HockeyCompetitionRules? rulesOverride = null)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));
        if (divisionId == Guid.Empty)
            throw new ArgumentException("Division id cannot be empty.", nameof(divisionId));
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Division name cannot be null or empty.", nameof(name));
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");

        CompetitionId = competitionId;
        DivisionId = divisionId;
        Name = name;
        SortOrder = sortOrder;
        IsActive = true;
        RulesOverride = rulesOverride;
    }

    /// <summary>Soft-deactivates the division link without removing historical data.</summary>
    internal void Deactivate() => IsActive = false;

    /// <summary>
    /// Adds a competition team to the division. Idempotent for an already-active membership.
    /// Competition-team validation is the caller's responsibility (competition aggregate).
    /// </summary>
    internal HockeyCompetitionDivisionTeam AddTeam(Guid competitionTeamId, int? seed = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add teams to an inactive division.");
        if (competitionTeamId == Guid.Empty)
            throw new ArgumentException("Competition team id cannot be empty.", nameof(competitionTeamId));

        HockeyCompetitionDivisionTeam? existing = _teams.FirstOrDefault(t => t.CompetitionTeamId == competitionTeamId && t.IsActive);
        if (existing is not null)
            return existing;

        HockeyCompetitionDivisionTeam divisionTeam = new(Id, competitionTeamId, seed);
        _teams.Add(divisionTeam);
        return divisionTeam;
    }

    /// <summary>Soft-removes a competition team from the division.</summary>
    internal void RemoveTeam(Guid competitionTeamId)
    {
        HockeyCompetitionDivisionTeam? existing = _teams.FirstOrDefault(t => t.CompetitionTeamId == competitionTeamId && t.IsActive)
            ?? throw new InvalidOperationException("Competition team is not part of this division.");

        existing.Deactivate();
    }

    /// <summary>Checks whether the given competition team has an active membership in this division.</summary>
    internal bool HasActiveTeam(Guid competitionTeamId) =>
        _teams.Any(t => t.CompetitionTeamId == competitionTeamId && t.IsActive);

    /// <summary>
    /// Records the division champion. The winner must be an active member of this division.
    /// </summary>
    internal void SetChampion(Guid championCompetitionTeamId)
    {
        if (championCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Champion competition team id cannot be empty.", nameof(championCompetitionTeamId));
        if (!HasActiveTeam(championCompetitionTeamId))
            throw new InvalidOperationException("Champion must be an active member of this division.");

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }
}
