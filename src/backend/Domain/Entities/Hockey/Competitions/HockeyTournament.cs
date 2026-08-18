using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a standalone hockey tournament with its own lifecycle and group-stage structure.
/// Owns <see cref="HockeyTournamentGroup"/> collections used during the group stage.
/// Teams enter groups via <see cref="AddTeamToGroup"/>, which requires an active
/// <see cref="HockeyCompetitionTeam"/> and enforces one active group per team.
/// </summary>
public class HockeyTournament : HockeyCompetition
{
    public string? ContentHtml { get; private set; }
    public string? Venue { get; private set; }
    public HockeyTournamentStage CurrentStage { get; private set; }
    public HockeyTournamentRules TournamentRules { get; private set; } = null!;
    public Guid? ChampionCompetitionTeamId { get; private set; }

    /// <summary>Gets the tournament groups (lohkot) used during the group stage.</summary>
    public IReadOnlyCollection<HockeyTournamentGroup> Groups => _groups.AsReadOnly();
    private protected readonly List<HockeyTournamentGroup> _groups = new();

    private HockeyTournament() : base()
    {
        TournamentRules = HockeyTournamentRules.Default();
        CurrentStage = HockeyTournamentStage.Registration;
    }

    public HockeyTournament(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? venue = null,
        string? contentHtml = null,
        HockeyTournamentRules? tournamentRules = null,
        HockeyCompetitionRules? competitionRules = null)
        : base(HockeyCompetitionType.Tournament, name, startDate, endDate, competitionRules)
    {
        Venue = venue;
        ContentHtml = contentHtml;
        TournamentRules = tournamentRules ?? HockeyTournamentRules.Default();
        CurrentStage = HockeyTournamentStage.Registration;
    }

    public void UpdateContent(string? contentHtml) => ContentHtml = contentHtml;

    public void UpdateVenue(string? venue) => Venue = venue;

    public void UpdateTournamentRules(HockeyTournamentRules tournamentRules)
    {
        ArgumentNullException.ThrowIfNull(tournamentRules);
        if (Status is HockeyCompetitionStatus.Completed or HockeyCompetitionStatus.Cancelled)
            throw new InvalidOperationException("Cannot update rules for a completed or cancelled tournament.");
        if (_matches.Count > 0)
            throw new InvalidOperationException("Cannot change tournament rules once matches have been created.");

        TournamentRules = tournamentRules;
    }

    public void AdvanceToGroupStage()
    {
        if (CurrentStage != HockeyTournamentStage.Registration)
            throw new InvalidOperationException($"Cannot start group stage when current stage is {CurrentStage}.");

        CurrentStage = HockeyTournamentStage.GroupStage;
        if (Status is HockeyCompetitionStatus.Published or HockeyCompetitionStatus.RegistrationOpen)
            Activate();
    }

    public void AdvanceToPlayoffs()
    {
        if (CurrentStage != HockeyTournamentStage.GroupStage)
            throw new InvalidOperationException($"Cannot start playoffs when current stage is {CurrentStage}.");
        if (!TournamentRules.HasPlayoffs)
            throw new InvalidOperationException("This tournament does not have playoffs.");

        CurrentStage = HockeyTournamentStage.Playoffs;
    }

    public void AdvanceToFinals()
    {
        if (CurrentStage != HockeyTournamentStage.Playoffs)
            throw new InvalidOperationException($"Cannot advance to finals when current stage is {CurrentStage}.");

        CurrentStage = HockeyTournamentStage.Finals;
    }

    public void CompleteTournament()
    {
        if (CurrentStage is not (HockeyTournamentStage.Finals or HockeyTournamentStage.Playoffs or HockeyTournamentStage.GroupStage))
            throw new InvalidOperationException($"Cannot complete tournament when current stage is {CurrentStage}.");

        CurrentStage = HockeyTournamentStage.Completed;
        Complete();
    }

    public void SetChampion(Guid championCompetitionTeamId)
    {
        if (championCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Champion competition team id cannot be empty.", nameof(championCompetitionTeamId));
        if (Status != HockeyCompetitionStatus.Completed)
            throw new InvalidOperationException("Champion can only be set for a completed tournament.");

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }

    public HockeyTournamentGroup AddGroup(string name)
    {
        EnsureMutable();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(name));

        int nextSortOrder = _groups.Count;
        HockeyTournamentGroup group = new(Id, name, nextSortOrder);
        _groups.Add(group);
        return group;
    }

    public void RemoveGroup(Guid groupId)
    {
        EnsureMutable();
        HockeyTournamentGroup? group = _groups.FirstOrDefault(g => g.Id == groupId)
            ?? throw new InvalidOperationException("Group is not part of this tournament.");

        _groups.Remove(group);
    }

    public HockeyTournamentGroup? GetGroup(Guid groupId) =>
        _groups.FirstOrDefault(g => g.Id == groupId);

    /// <summary>
    /// Adds a competition team to a group. Validates that the team is an active
    /// member of this tournament and not already in another active group.
    /// </summary>
    public HockeyTournamentGroupTeam AddTeamToGroup(Guid groupId, Guid competitionTeamId, int? seed = null)
    {
        EnsureMutable();
        ValidateCompetitionTeam(competitionTeamId);

        if (_groups.Any(g => g.HasActiveTeam(competitionTeamId)))
            throw new InvalidOperationException("Competition team is already assigned to a group.");

        HockeyTournamentGroup group = GetGroup(groupId)
            ?? throw new InvalidOperationException("Group is not part of this tournament.");

        return group.AddTeam(competitionTeamId, seed);
    }

    public void RemoveTeamFromGroup(Guid groupId, Guid competitionTeamId)
    {
        EnsureMutable();
        HockeyTournamentGroup group = GetGroup(groupId)
            ?? throw new InvalidOperationException("Group is not part of this tournament.");

        group.RemoveTeam(competitionTeamId);
    }

    /// <summary>
    /// Extends base removal checks with active tournament group memberships.
    /// A team cannot leave the competition while still placed in a group.
    /// </summary>
    private protected override bool HasBlockingTeamReferences(HockeyCompetitionTeam competitionTeam)
    {
        if (base.HasBlockingTeamReferences(competitionTeam))
            return true;

        return _groups.Any(g => g.HasActiveTeam(competitionTeam.Id));
    }

    public override HockeyCompetitionRules GetEffectiveRules()
    {
        if (TournamentRules.MatchRulesOverride is null)
            return CompetitionRules;

        return CompetitionRules.WithMatchRules(TournamentRules.MatchRulesOverride);
    }
}
