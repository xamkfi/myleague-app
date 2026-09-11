namespace Application.Features.Football.Statistics.DTOs;

/// <summary>
/// Per-team standings row for a tournament group, computed from completed group-stage matches.
/// </summary>
public record FootballTournamentGroupStandingDto(
    Guid TeamId,
    string TeamName,
    Uri? TeamLogo,
    int GamesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
