using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Teams.Mappings;

/// <summary>
/// Mapper for FloorballTeam entity
/// </summary>
public static class FloorballTeamMapper
{
    /// <summary>
    /// Maps a FloorballTeam entity to a FloorballTeamDto
    /// </summary>
    /// <param name="team">The team entity to map</param>
    /// <param name="club">The club entity (optional, since Club navigation is ignored in EF)</param>
    /// <param name="playerPersons">Dictionary of player persons keyed by player ID</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when team is null</exception>
    public static FloorballTeamDto ToDto(FloorballTeam team, Club? club, Dictionary<Guid, Person>? playerPersons)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        // Use the provided club parameter, or throw an exception if it's null
        if (club == null)
            throw new ArgumentNullException(nameof(club), "Club must be provided since the Club navigation property is ignored in EF configuration");

        if (playerPersons == null)
            throw new ArgumentNullException(nameof(playerPersons));

        string? effectiveLogoUrl = team.GetEffectiveLogoUrl(club.LogoUrl)?.ToString();

        return new FloorballTeamDto(
            team.Id,
            team.Name,
            team.ShortName,
            team.DivisionId,
            ClubMapper.ToDto(club),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            effectiveLogoUrl,
            team.HasActiveMembers,
            team.Roster.Select(p => 
            {
                string playerName = "Unknown Player";
                int? age = null;
                if (playerPersons != null && playerPersons.TryGetValue(p.PlayerId, out Person? person))
                {
                    playerName = person.FullName;
                    if (person.BirthDate.HasValue)
                    {
                        DateTime today = DateTime.UtcNow;
                        age = today.Year - person.BirthDate.Value.Year;
                        if (person.BirthDate.Value.Date > today.AddYears(-age.Value))
                            age--;
                    }
                }
                
                return new FloorballTeamPlayerDto(
                    team.Id,
                    p.PlayerId,
                    playerName,
                    p.Position,
                    p.JerseyNumber,
                    p.IsActive,
                    GamesPlayed: p.GamesPlayed,
                    Goals: p.Goals,
                    Assists: p.Assists,
                    PenaltyMinutes: p.PenaltyMinutes,
                    Age: age,
                    // Only surface when there's actually a mismatch — the UI uses this to drive
                    // the "needs admin review" highlight on the roster page.
                    RequestedJerseyNumber: p.HasJerseyNumberSubstituted ? p.RequestedJerseyNumber : null
                );
            }).ToList().AsReadOnly()
        );
    }
    // Without player names (for existing handlers like Update)
    public static FloorballTeamDto ToDto(FloorballTeam team, Club club)
        => ToDto(team, club, new Dictionary<Guid, Person>());

    /// <summary>
    /// Maps a collection of FloorballTeam entities to FloorballTeamDtos
    /// </summary>
    /// <param name="teams">The team entities to map</param>
    /// <param name="clubs">Dictionary of clubs keyed by club ID</param>
    /// <param name="playerPersons">Dictionary of player persons keyed by player ID</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when teams is null</exception>
    public static IEnumerable<FloorballTeamDto> ToDtos(IEnumerable<FloorballTeam> teams, Dictionary<Guid, Club>? clubs = null, Dictionary<Guid, Person>? playerPersons = null)
    {
        if (teams == null)
            throw new ArgumentNullException(nameof(teams));

        return teams.Select(team => 
        {
            Club? club = null;
            clubs?.TryGetValue(team.ClubId, out club);
            return ToDto(team, club, playerPersons);
        });
    }

    /// <summary>
    /// Creates a new FloorballTeam entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="NotImplementedException">This method requires Club entity to be loaded separately</exception>
    public static FloorballTeam ToEntity(CreateFloorballTeamCommand command, Club club)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // TODO: Need to load Club entity from command.ClubId
        // This method should be updated to either:
        // 1. Accept a Club parameter in addition to the command, or
        // 2. Load the Club entity within this method using a repository
        // throw new NotImplementedException("This method requires a Club entity to be provided. The FloorballTeam constructor needs a Club entity, but the command only contains ClubId.");

        return new FloorballTeam(
            command.Name,
            command.DivisionId,
            club,
            command.HomeArena,
            command.PrimaryJerseyColor,
            command.TeamCategory ?? Domain.Enums.Common.TeamCategory.Adult,
            command.SecondaryJerseyColor,
            command.ShortName
        );
    }

    /// <summary>
    /// Updates a FloorballTeam entity from an update command
    /// </summary>
    /// <param name="team">The team entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when team or command is null</exception>
    public static void UpdateFromCommand(FloorballTeam team, UpdateFloorballTeamCommand command)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Use the entity's public update methods
        team.UpdateName(command.Name);
        team.UpdateDivision(command.DivisionId);
        team.UpdateHomeArena(command.HomeArena);
        team.UpdateJerseyColors(command.PrimaryJerseyColor, command.SecondaryJerseyColor);
        team.UpdateShortName(command.ShortName);
        if (command.TeamCategory.HasValue)
        {
            team.UpdateTeamCategory(command.TeamCategory.Value);
        }
        
        // Update logo URL
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;
        team.UpdateLogo(logoUri);
    }

    /// <summary>
    /// Maps a FloorballTeam entity to a FloorballTeamSummaryDto (without roster)
    /// </summary>
    /// <param name="team">The team entity to map</param>
    /// <param name="club">The club entity (optional, since Club navigation is ignored in EF)</param>
    /// <returns>The mapped summary DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when team is null</exception>
    public static FloorballTeamSummaryDto ToSummaryDto(FloorballTeam team, Club? club)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        // Use the provided club parameter, or throw an exception if it's null
        if (club == null)
            throw new ArgumentNullException(nameof(club), "Club must be provided since the Club navigation property is ignored in EF configuration");

        string? effectiveLogoUrl = team.GetEffectiveLogoUrl(club.LogoUrl)?.ToString();

        return new FloorballTeamSummaryDto(
            team.Id,
            team.Name,
            team.DivisionId,
            ClubMapper.ToDto(club),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            effectiveLogoUrl,
            team.HasActiveMembers,
            team.TeamCategory);
    }

    /// <summary>
    /// Maps a collection of FloorballTeam entities to FloorballTeamSummaryDtos (without roster)
    /// </summary>
    /// <param name="teams">The team entities to map</param>
    /// <param name="clubs">Dictionary of clubs keyed by club ID</param>
    /// <returns>The mapped summary DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when teams is null</exception>
    public static IEnumerable<FloorballTeamSummaryDto> ToSummaryDtos(IEnumerable<FloorballTeam> teams, Dictionary<Guid, Club>? clubs = null)
    {
        if (teams == null)
            throw new ArgumentNullException(nameof(teams));

        return teams.Select(team => 
        {
            Club? club = null;
            clubs?.TryGetValue(team.ClubId, out club);
            return ToSummaryDto(team, club);
        });
    }
} 
