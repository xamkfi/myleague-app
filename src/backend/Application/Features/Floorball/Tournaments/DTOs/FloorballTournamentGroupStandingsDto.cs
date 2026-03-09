namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for a group's standings table (sarjataulukko)
/// </summary>
/// <param name="GroupId">The group identifier</param>
/// <param name="GroupName">The display name of the group</param>
/// <param name="Entries">Ranked standings entries for each team in the group</param>
public record FloorballTournamentGroupStandingsDto(
    Guid GroupId,
    string GroupName,
    IReadOnlyCollection<FloorballTournamentGroupStandingEntryDto> Entries);

/// <summary>
/// A single team's row in the group standings table
/// </summary>
/// <param name="Rank">Position in the standings</param>
/// <param name="TeamId">The team identifier</param>
/// <param name="TeamName">The display name of the team</param>
/// <param name="GamesPlayed">Total games played</param>
/// <param name="Wins">Total wins</param>
/// <param name="Draws">Total draws</param>
/// <param name="Losses">Total losses</param>
/// <param name="GoalsFor">Total goals scored</param>
/// <param name="GoalsAgainst">Total goals conceded</param>
/// <param name="GoalDifference">Goal difference (GoalsFor - GoalsAgainst)</param>
/// <param name="Points">Total points earned</param>
public record FloorballTournamentGroupStandingEntryDto(
    int Rank,
    Guid TeamId,
    string TeamName,
    int GamesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
