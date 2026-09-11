using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Per-match goalie box score keyed by match active player.
/// </summary>
public class HockeyGoalieMatchStatistics : BaseEntity
{
    public Guid MatchId { get; private set; }
    public HockeyMatch Match { get; private set; } = null!;

    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public Guid MatchActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer MatchActivePlayer { get; private set; } = null!;

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

    public Guid PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public bool WasStarter { get; private set; }
    public HockeyGoalieDecision Decision { get; private set; }
    public int GamesPlayed { get; private set; }
    public int GamesStarted { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int OvertimeLosses { get; private set; }
    public int ShootoutLosses { get; private set; }
    public int NoDecisions { get; private set; }
    public int Saves { get; private set; }
    public int ShotsAgainst { get; private set; }
    public decimal SavePercentage { get; private set; }
    public int GoalsAgainst { get; private set; }
    public decimal GoalsAgainstAverage { get; private set; }
    public int Shutouts { get; private set; }
    public int MinutesPlayed { get; private set; }

    public IReadOnlyCollection<HockeyGoaliePeriodStatistics> PeriodStatistics => _periodStatistics.AsReadOnly();
    private readonly List<HockeyGoaliePeriodStatistics> _periodStatistics = new();

    private HockeyGoalieMatchStatistics() { }

    public HockeyGoalieMatchStatistics(
        Guid matchId,
        Guid matchTeamId,
        Guid matchActivePlayerId,
        Guid teamPlayerId,
        Guid playerId,
        Guid teamId,
        bool wasStarter = false,
        HockeyGoalieDecision decision = HockeyGoalieDecision.NoDecision)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        MatchId = matchId;
        MatchTeamId = matchTeamId;
        MatchActivePlayerId = matchActivePlayerId;
        TeamPlayerId = teamPlayerId;
        PlayerId = playerId;
        TeamId = teamId;
        WasStarter = wasStarter;
        Decision = decision;
        GamesPlayed = 1;
        GamesStarted = wasStarter ? 1 : 0;
        ApplyDecisionCounters(decision);
        RecalculateDerived();
    }

    public void SetDecision(HockeyGoalieDecision decision)
    {
        ClearDecisionCounters();
        Decision = decision;
        ApplyDecisionCounters(decision);
    }

    public void SetWasStarter(bool wasStarter)
    {
        WasStarter = wasStarter;
        GamesStarted = wasStarter ? 1 : 0;
    }

    public void UpdateGoaltending(int saves, int shotsAgainst, int goalsAgainst, int minutesPlayed, int shutouts)
    {
        HockeyStatisticsMath.EnsureNonNegative(saves, nameof(saves));
        HockeyStatisticsMath.EnsureNonNegative(shotsAgainst, nameof(shotsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(goalsAgainst, nameof(goalsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(minutesPlayed, nameof(minutesPlayed));
        HockeyStatisticsMath.EnsureNonNegative(shutouts, nameof(shutouts));
        Saves = saves;
        ShotsAgainst = shotsAgainst;
        GoalsAgainst = goalsAgainst;
        MinutesPlayed = minutesPlayed;
        Shutouts = shutouts;
        RecalculateDerived();
    }

    public HockeyGoaliePeriodStatistics AddPeriodStatistics(int periodNumber, HockeyPeriodType periodType)
    {
        if (_periodStatistics.Any(p => p.PeriodNumber == periodNumber))
            throw new InvalidOperationException($"Period statistics for period {periodNumber} already exist.");

        HockeyGoaliePeriodStatistics period = new(
            Id,
            MatchId,
            MatchTeamId,
            MatchActivePlayerId,
            TeamPlayerId,
            PlayerId,
            TeamId,
            periodNumber,
            periodType);
        _periodStatistics.Add(period);
        return period;
    }

    private void ApplyDecisionCounters(HockeyGoalieDecision decision)
    {
        switch (decision)
        {
            case HockeyGoalieDecision.Win:
                Wins = 1;
                break;
            case HockeyGoalieDecision.Loss:
                Losses = 1;
                break;
            case HockeyGoalieDecision.OvertimeLoss:
                OvertimeLosses = 1;
                break;
            case HockeyGoalieDecision.ShootoutLoss:
                ShootoutLosses = 1;
                break;
            case HockeyGoalieDecision.NoDecision:
                NoDecisions = 1;
                break;
            case HockeyGoalieDecision.Tie:
                break;
        }
    }

    private void ClearDecisionCounters()
    {
        Wins = 0;
        Losses = 0;
        OvertimeLosses = 0;
        ShootoutLosses = 0;
        NoDecisions = 0;
    }

    private void RecalculateDerived()
    {
        SavePercentage = HockeyStatisticsMath.Percentage(Saves, ShotsAgainst);
        GoalsAgainstAverage = HockeyStatisticsMath.GoalsAgainstAverage(GoalsAgainst, MinutesPlayed);
    }
}
