using Application.Features.Floorball.TeamManagers.Commands;
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
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.TeamManagers.Mappings;

/// <summary>
/// Mapper for FloorballTeamManager entity
/// </summary>
public static class FloorballTeamManagerMapper
{
    /// <summary>
    /// Maps a FloorballTeamManager entity to a FloorballTeamManagerDto
    /// </summary>
    /// <param name="manager">The team manager entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static FloorballTeamManagerDto ToDto(FloorballTeamManager manager)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));

        return new FloorballTeamManagerDto(
            manager.Id,
            manager.PersonId,
            null!, // TODO: Add Person navigation property to FloorballTeamManager entity or load Person separately
            manager.TeamId,
            manager.IsActive
        );
    }

    /// <summary>
    /// Maps a collection of FloorballTeamManager entities to FloorballTeamManagerDtos
    /// </summary>
    /// <param name="managers">The team manager entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when managers is null</exception>
    public static IEnumerable<FloorballTeamManagerDto> ToDtos(IEnumerable<FloorballTeamManager> managers)
    {
        if (managers == null)
            throw new ArgumentNullException(nameof(managers));

        return managers.Select(manager => ToDto(manager));
    }

    /// <summary>
    /// Creates a new FloorballTeamManager entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team manager entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballTeamManager ToEntity(CreateFloorballTeamManagerCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballTeamManager(
            command.PersonId,
            command.TeamId);
    }

    /// <summary>
    /// Updates a FloorballTeamManager entity from an update command
    /// </summary>
    /// <param name="manager">The team manager entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when manager or command is null</exception>
    public static void UpdateFromCommand(FloorballTeamManager manager, UpdateFloorballTeamManagerCommand command)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        manager.UpdateActiveStatus(command.IsActive);
    }
} 
