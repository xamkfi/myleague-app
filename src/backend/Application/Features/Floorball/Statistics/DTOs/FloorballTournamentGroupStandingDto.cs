using System;

namespace Application.Features.Floorball.Statistics.DTOs;

/// <summary>
/// Per-team standings row for a tournament group, computed from completed group-stage matches.
/// </summary>
/// <param name="TeamId">The team's unique identifier</param>
/// <param name="TeamName">The team's display name</param>
/// <param name="TeamLogo">The team logo URL (with club fallback applied)</param>
/// <param name="GamesPlayed">Number of completed group-stage games this team has played</param>
/// <param name="Wins">Number of wins (regulation, overtime or shootout)</param>
/// <param name="Draws">Number of draws (only possible if overtime/shootout disabled)</param>
/// <param name="Losses">Number of losses</param>
/// <param name="GoalsFor">Total goals scored by the team in this group</param>
/// <param name="GoalsAgainst">Total goals conceded by the team in this group</param>
/// <param name="GoalDifference">GoalsFor minus GoalsAgainst</param>
/// <param name="Points">Standings points (3 for a win, 1 for a draw, 0 for a loss)</param>
public record FloorballTournamentGroupStandingDto(
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
