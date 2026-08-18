using System;
using Application.Features.Football.Players.DTOs;
using Domain.Enums.Football;

namespace Application.Features.Football.Teams.DTOs
{
    /// <summary>
    /// Data Transfer Object for FootballTeamPlayer value object
    /// </summary>
    /// <param name="TeamId">The ID of the team</param>
    /// <param name="PlayerId">The ID of the player</param>
    /// <param name="PlayerName">The name of the player</param>
    /// <param name="Position">The player's position in the team</param>
    /// <param name="JerseyNumber">The player's jersey number (if assigned)</param>
    /// <param name="IsActive">Whether the player is currently active</param>
    /// <param name="Player">The full player information (optional)</param>
    /// <param name="GamesPlayed">Number of games played for this team</param>
    /// <param name="Goals">Number of goals scored for this team</param>
    /// <param name="Assists">Number of assists made for this team</param>
    /// <param name="YellowCards">Number of yellow cards for this team</param>
    /// <param name="RedCards">Number of red cards for this team</param>
    /// <param name="Age">The player's age (null if birth date is unknown)</param>
    public record FootballTeamPlayerDto(
        Guid TeamId,
        Guid PlayerId,
        string PlayerName,
        FootballPosition Position,
        int? JerseyNumber,
        bool IsActive,
        FootballPlayerDto? Player = null,
        int GamesPlayed = 0,
        int Goals = 0,
        int Assists = 0,
        int YellowCards = 0,
        int RedCards = 0,
        int? Age = null,
        /// <summary>
        /// The jersey number originally requested for this player when the assigned number
        /// is a substitute (i.e. the requested one was taken on the team). <c>null</c> means
        /// the assigned number matches the requested one and no admin review is needed.
        /// </summary>
        int? RequestedJerseyNumber = null);
} 
