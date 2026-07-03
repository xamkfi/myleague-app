namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Minimal hockey match aggregate placeholder for competition membership.
/// Full match behaviour is implemented in a separate ticket.
/// </summary>
public class HockeyMatch : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public Guid? HomeCompetitionTeamId { get; private set; }
    public Guid? AwayCompetitionTeamId { get; private set; }

    private HockeyMatch() { }

    public HockeyMatch(Guid competitionId, Guid? homeCompetitionTeamId = null, Guid? awayCompetitionTeamId = null)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));

        CompetitionId = competitionId;
        HomeCompetitionTeamId = homeCompetitionTeamId;
        AwayCompetitionTeamId = awayCompetitionTeamId;
    }

    public bool ReferencesCompetitionTeam(Guid competitionTeamId) =>
        HomeCompetitionTeamId == competitionTeamId || AwayCompetitionTeamId == competitionTeamId;
}
