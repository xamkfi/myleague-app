using Domain.Enums.Hockey.Competitions;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a best-of playoff series within a competition.
/// </summary>
public class HockeyPlayoffSeries : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public HockeyCompetition Competition { get; private set; } = null!;
    public HockeyPlayoffRound Round { get; private set; }
    public int SeriesOrder { get; private set; }
    public int BestOf { get; private set; }
    public Guid? HomeCompetitionTeamId { get; private set; }
    public Guid? AwayCompetitionTeamId { get; private set; }
    public int HomeTeamWins { get; private set; }
    public int AwayTeamWins { get; private set; }
    public Guid? WinnerCompetitionTeamId { get; private set; }
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
        if (bestOf < 1 || bestOf % 2 == 0)
            throw new ArgumentOutOfRangeException(nameof(bestOf), "Best-of value must be a positive odd number.");

        CompetitionId = competitionId;
        Round = round;
        SeriesOrder = seriesOrder;
        BestOf = bestOf;
        HomeCompetitionTeamId = homeCompetitionTeamId;
        AwayCompetitionTeamId = awayCompetitionTeamId;
        Status = HockeyPlayoffSeriesStatus.NotStarted;
    }
}
