namespace Domain.Common;

/// <summary>
/// One competition statistics row used as input when summing all-time player totals.
/// Loan placeholders stay in the row so the ranking step can drop them.
/// </summary>
public sealed record AllTimePlayerStatRow(
    Guid PlayerId,
    Guid PersonId,
    bool IsLoanProfile,
    Guid TeamId,
    string TeamName,
    DateTime CompetitionStart,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points,
    int PenaltyMinutes,
    int YellowCards,
    int RedCards);
