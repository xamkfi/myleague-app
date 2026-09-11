using Application.Features.Hockey.Players.DTOs;
using Domain.Entities.Hockey.Teams;

namespace Application.Features.Hockey.Players.Mappings;

/// <summary>
/// Maps hockey player domain entities to application DTOs.
/// </summary>
public static class HockeyPlayerMapper
{
    /// <summary>
    /// Maps a hockey player to a DTO.
    /// </summary>
    public static HockeyPlayerDto ToDto(HockeyPlayer player)
    {
        return new HockeyPlayerDto(
            player.Id,
            player.PersonId,
            player.LicenseNumber,
            player.IsActive,
            player.PrimaryPosition.ToString(),
            player.Shoots.ToString(),
            player.Catches?.ToString(),
            player.CareerGamesPlayed,
            player.CareerGoals,
            player.CareerAssists,
            player.CareerPenaltyMinutes);
    }
}
