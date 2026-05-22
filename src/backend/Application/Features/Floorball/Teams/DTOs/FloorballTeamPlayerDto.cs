using System;
using Application.Features.Floorball.Players.DTOs;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Teams.DTOs
{
    /// <summary>
    /// Data Transfer Object for FloorballTeamPlayer value object
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
    /// <param name="PenaltyMinutes">Number of penalty minutes for this team</param>
    /// <param name="Age">The player's age (null if birth date is unknown)</param>
    public record FloorballTeamPlayerDto(
        Guid TeamId,
        Guid PlayerId,
        string PlayerName,
        FloorballPosition Position,
        int? JerseyNumber,
        bool IsActive,
        FloorballPlayerDto? Player = null,
        int GamesPlayed = 0,
        int Goals = 0,
        int Assists = 0,
        int PenaltyMinutes = 0,
        int? Age = null,
        /// <summary>
        /// The jersey number originally requested for this player when the assigned number
        /// is a substitute (i.e. the requested one was taken on the team). <c>null</c> means
        /// the assigned number matches the requested one and no admin review is needed.
        /// </summary>
        int? RequestedJerseyNumber = null);
} 
