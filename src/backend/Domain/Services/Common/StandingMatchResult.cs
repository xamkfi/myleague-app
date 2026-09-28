namespace Domain.Services.Common;

/// <summary>
/// One finished match used when ranking teams by their games against each other.
/// </summary>
public readonly record struct StandingMatchResult(
    Guid HomeTeamId,
    Guid AwayTeamId,
    int HomeGoals,
    int AwayGoals,
    int HomePoints,
    int AwayPoints);
