using Application.Commands.Floorball.Team;
using Application.DTOs.Floorball;
using Application.Mappings.Common;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

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
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when team is null</exception>
    public static FloorballTeamDto ToDto(FloorballTeam team)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        return new FloorballTeamDto(
            team.Id,
            team.Name,
            team.Division,
            ClubMapper.ToDto(team.Club),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            team.HasActiveMembers,
            team.Roster.Select(player => new FloorballTeamPlayerDto(
                team.Id,
                player.PlayerId,
                "", // TODO: Need player name - requires loading Player entity or separate mapping
                player.Position,
                player.JerseyNumber,
                player.IsActive
            )).ToList().AsReadOnly()
        );
    }

    /// <summary>
    /// Maps a collection of FloorballTeam entities to FloorballTeamDtos
    /// </summary>
    /// <param name="teams">The team entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when teams is null</exception>
    public static IEnumerable<FloorballTeamDto> ToDtos(IEnumerable<FloorballTeam> teams)
    {
        if (teams == null)
            throw new ArgumentNullException(nameof(teams));

        return teams.Select(team => ToDto(team));
    }

    /// <summary>
    /// Creates a new FloorballTeam entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="NotImplementedException">This method requires Club entity to be loaded separately</exception>
    public static FloorballTeam ToEntity(CreateFloorballTeamCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // TODO: Need to load Club entity from command.ClubId
        // This method should be updated to either:
        // 1. Accept a Club parameter in addition to the command, or
        // 2. Load the Club entity within this method using a repository
        throw new NotImplementedException("This method requires a Club entity to be provided. The FloorballTeam constructor needs a Club entity, but the command only contains ClubId.");
        
        // When Club is available, use this constructor:
        // return new FloorballTeam(
        //     command.Name,
        //     command.Division,
        //     club, // Club entity loaded from command.ClubId
        //     command.HomeArena,
        //     command.PrimaryJerseyColor,
        //     command.SecondaryJerseyColor
        // );
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
        team.UpdateDivision(command.Division);
        team.UpdateHomeArena(command.HomeArena);
        team.UpdateJerseyColors(command.PrimaryJerseyColor, command.SecondaryJerseyColor!);
    }
} 
