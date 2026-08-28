using Domain.Enums.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// A disciplinary card shown during a football match.
/// </summary>
public class FootballCard : FootballMatchEvent
{
    public Guid PlayerId { get; private set; }
    public FootballCardType CardType { get; private set; }

    private FootballCard()
    {
    }

    public FootballCard(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        FootballCardType cardType,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));

        PlayerId = playerId;
        CardType = cardType;
    }

    public bool ResultsInSendingOff =>
        CardType == FootballCardType.SecondYellow || CardType == FootballCardType.DirectRed;
}
