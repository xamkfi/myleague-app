using Domain.Enums.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// Per-period (half / extra-time / shootout) score snapshot.
/// </summary>
public class FootballPeriodScore : BaseEntity
{
    public Guid MatchId { get; private set; }
    public int PeriodNumber { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public int HomeScore { get; private set; }
    public int AwayScore { get; private set; }
    public bool IsCompleted { get; private set; }

    private FootballPeriodScore()
    {
    }

    public FootballPeriodScore(Guid matchId, int periodNumber, Guid homeTeamId, Guid awayTeamId)
    {
        if (periodNumber <= 0)
            throw new ArgumentException("Period number must be positive.", nameof(periodNumber));

        MatchId = matchId;
        PeriodNumber = periodNumber;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
    }

    public void UpdateHomeScore(int score)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(score));
        HomeScore = score;
    }

    public void UpdateAwayScore(int score)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(score));
        AwayScore = score;
    }

    public void IncrementHomeScore() => HomeScore++;
    public void IncrementAwayScore() => AwayScore++;

    public void DecrementHomeScore()
    {
        if (HomeScore <= 0)
            throw new InvalidOperationException("Cannot decrement home score below 0.");
        HomeScore--;
    }

    public void DecrementAwayScore()
    {
        if (AwayScore <= 0)
            throw new InvalidOperationException("Cannot decrement away score below 0.");
        AwayScore--;
    }

    public void UpdateTeamId(FootballPlayoffSlot slot, Guid teamId)
    {
        if (slot == FootballPlayoffSlot.Home)
            HomeTeamId = teamId;
        else
            AwayTeamId = teamId;
    }

    public void Complete() => IsCompleted = true;
    public void Reopen() => IsCompleted = false;
}
