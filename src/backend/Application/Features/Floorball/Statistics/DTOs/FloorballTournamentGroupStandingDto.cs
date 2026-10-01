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
/// <param name="TeamShortName">Short name shown when the team has no logo</param>
/// <param name="RegulationWins">Wins decided in regulation time</param>
/// <param name="OvertimeWins">Wins decided in overtime without a shootout</param>
/// <param name="ShootoutWins">Wins decided by a shootout</param>
/// <param name="RegulationLosses">Losses decided in regulation time</param>
/// <param name="OvertimeLosses">Losses decided in overtime without a shootout</param>
/// <param name="ShootoutLosses">Losses decided by a shootout</param>
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
    int Points,
    string TeamShortName,
    int RegulationWins = 0,
    int OvertimeWins = 0,
    int ShootoutWins = 0,
    int RegulationLosses = 0,
    int OvertimeLosses = 0,
    int ShootoutLosses = 0);
