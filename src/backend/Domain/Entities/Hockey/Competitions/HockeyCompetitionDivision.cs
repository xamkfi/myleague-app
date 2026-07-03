using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Links a hockey competition to a division from the common context.
/// </summary>
public class HockeyCompetitionDivision : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public HockeyCompetition Competition { get; private set; } = null!;
    public Guid DivisionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? ChampionCompetitionTeamId { get; private set; }
    public HockeyCompetitionRules? RulesOverride { get; private set; }

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

    internal void Deactivate() => IsActive = false;

    internal void SetChampion(Guid championCompetitionTeamId)
    {
        if (championCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Champion competition team id cannot be empty.", nameof(championCompetitionTeamId));

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }
}
