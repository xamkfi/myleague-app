using Domain.Enums.Hockey.Competitions;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// A best-of playoff series within a <see cref="HockeyCompetition"/>.
/// Home, away and winner all reference <see cref="HockeyCompetitionTeam"/> — not
/// <c>HockeyTeam</c> directly. Series are created via
/// <see cref="HockeyCompetition.CreatePlayoffSeries"/> which validates that assigned
/// teams are active members of the same competition. Teams can be assigned later via
/// <see cref="HockeyCompetition.AssignPlayoffSeriesTeams"/> while
/// <see cref="Status"/> is <see cref="HockeyPlayoffSeriesStatus.NotStarted"/>.
/// <see cref="BestOf"/> must be at least 1 (even values like best-of-2 are allowed).
/// </summary>
public class HockeyPlayoffSeries : BaseEntity
{
    /// <summary>Gets the competition this series belongs to.</summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>Gets the parent competition aggregate.</summary>
    public HockeyCompetition Competition { get; private set; } = null!;

    /// <summary>Gets the playoff round (e.g. quarter-final, semi-final).</summary>
    public HockeyPlayoffRound Round { get; private set; }

    /// <summary>Gets the ordering of this series within the round.</summary>
    public int SeriesOrder { get; private set; }

    /// <summary>
    /// Gets the best-of format (minimum 1). Determines how many wins are needed
    /// to decide the series winner.
    /// </summary>
    public int BestOf { get; private set; }

    /// <summary>Gets the home competition team's id, if assigned.</summary>
    public Guid? HomeCompetitionTeamId { get; private set; }

    /// <summary>Gets the home competition team, if assigned.</summary>
    public HockeyCompetitionTeam? HomeCompetitionTeam { get; private set; }

    /// <summary>Gets the away competition team's id, if assigned.</summary>
    public Guid? AwayCompetitionTeamId { get; private set; }

    /// <summary>Gets the away competition team, if assigned.</summary>
    public HockeyCompetitionTeam? AwayCompetitionTeam { get; private set; }

    /// <summary>Gets the number of wins by the home team in this series.</summary>
    public int HomeTeamWins { get; private set; }

    /// <summary>Gets the number of wins by the away team in this series.</summary>
    public int AwayTeamWins { get; private set; }

    /// <summary>Gets the winning competition team's id, set when the series completes.</summary>
    public Guid? WinnerCompetitionTeamId { get; private set; }

    /// <summary>Gets the winning competition team, set when the series completes.</summary>
    public HockeyCompetitionTeam? WinnerCompetitionTeam { get; private set; }

    /// <summary>Gets the current lifecycle status of the series.</summary>
    public HockeyPlayoffSeriesStatus Status { get; private set; }

    private HockeyPlayoffSeries() { }

    internal HockeyPlayoffSeries(
        Guid competitionId,
        HockeyPlayoffRound round,
        int seriesOrder,
        int bestOf,
        Guid? homeCompetitionTeamId = null,
        Guid? awayCompetitionTeamId = null)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));
        if (seriesOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(seriesOrder), "Series order cannot be negative.");
        if (bestOf < 1)
            throw new ArgumentOutOfRangeException(nameof(bestOf), "Best-of value must be at least 1.");

        CompetitionId = competitionId;
        Round = round;
        SeriesOrder = seriesOrder;
        BestOf = bestOf;
        HomeCompetitionTeamId = homeCompetitionTeamId;
        AwayCompetitionTeamId = awayCompetitionTeamId;
        Status = HockeyPlayoffSeriesStatus.NotStarted;
    }

    /// <summary>
    /// Assigns home and away teams. Only allowed before the series starts.
    /// Home and away must be different competition teams.
    /// </summary>
    internal void AssignTeams(Guid homeCompetitionTeamId, Guid awayCompetitionTeamId)
    {
        if (Status != HockeyPlayoffSeriesStatus.NotStarted)
            throw new InvalidOperationException("Teams can only be assigned before the series starts.");
        if (homeCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Home competition team id cannot be empty.", nameof(homeCompetitionTeamId));
        if (awayCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Away competition team id cannot be empty.", nameof(awayCompetitionTeamId));
        if (homeCompetitionTeamId == awayCompetitionTeamId)
            throw new InvalidOperationException("Home and away teams must be different.");

        HomeCompetitionTeamId = homeCompetitionTeamId;
        AwayCompetitionTeamId = awayCompetitionTeamId;
    }

    /// <summary>
    /// Records the series winner. The winner must be either the home or away team.
    /// Sets <see cref="Status"/> to <see cref="HockeyPlayoffSeriesStatus.Completed"/>.
    /// </summary>
    internal void SetWinner(Guid winnerCompetitionTeamId)
    {
        if (winnerCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Winner competition team id cannot be empty.", nameof(winnerCompetitionTeamId));
        if (winnerCompetitionTeamId != HomeCompetitionTeamId && winnerCompetitionTeamId != AwayCompetitionTeamId)
            throw new InvalidOperationException("Winner must be either the home or away competition team.");

        WinnerCompetitionTeamId = winnerCompetitionTeamId;
        Status = HockeyPlayoffSeriesStatus.Completed;
    }

    /// <summary>
    /// Checks whether the given competition team is assigned as home or away in this series.
    /// Used by <see cref="HockeyCompetition"/> to block team removal while referenced.
    /// </summary>
    internal bool ReferencesCompetitionTeam(Guid competitionTeamId) =>
        HomeCompetitionTeamId == competitionTeamId || AwayCompetitionTeamId == competitionTeamId;
}
