using Application.Commands.Floorball.Team;
using Application.DTOs.Floorball;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Mappings.Floorball;

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
    public static FloorballTeamDto ToDto(FloorballTeam team, Club club, Dictionary<Guid, Person>? playerPersons)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        // Use the provided club parameter, or throw an exception if it's null
        if (club == null)
            throw new ArgumentNullException(nameof(club), "Club must be provided since the Club navigation property is ignored in EF configuration");

        if (playerPersons == null)
            throw new ArgumentNullException(nameof(playerPersons));

        return new FloorballTeamDto(
            team.Id,
            team.Name,
            team.DivisionId,
            ClubMapper.ToDto(club),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            team.HasActiveMembers,
            team.Roster.Select(p => 
            {
                string playerName = "Unknown Player";
                if (playerPersons != null && playerPersons.TryGetValue(p.PlayerId, out Person? person))
                {
                    playerName = person.FullName;
                }
                
                return new FloorballTeamPlayerDto(
                team.Id,
                p.PlayerId,
                    playerName,
                p.Position,
                p.JerseyNumber,
                p.IsActive
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
            command.TeamCategory,
            command.SecondaryJerseyColor
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
        team.UpdateJerseyColors(command.PrimaryJerseyColor, command.SecondaryJerseyColor!);
    }
} 
