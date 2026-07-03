using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a standalone hockey tournament with its own lifecycle.
/// </summary>
public class HockeyTournament : HockeyCompetition
{
    public string? ContentHtml { get; private set; }
    public string? Venue { get; private set; }
    public HockeyTournamentStage CurrentStage { get; private set; }
    public HockeyTournamentRules TournamentRules { get; private set; } = null!;
    public Guid? ChampionCompetitionTeamId { get; private set; }

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

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }

    public override HockeyCompetitionRules GetEffectiveRules()
    {
        if (TournamentRules.MatchRulesOverride is null)
            return CompetitionRules;

        return CompetitionRules.WithMatchRules(TournamentRules.MatchRulesOverride);
    }
}
