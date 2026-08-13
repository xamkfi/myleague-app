using Application.Features.Football.TeamManagers.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Domain.Entities.Football.Teams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Football.TeamManagers.Mappings;

/// <summary>
/// Mapper for FootballTeamManager entity
/// </summary>
public static class FootballTeamManagerMapper
{
    /// <summary>
    /// Maps a FootballTeamManager entity to a FootballTeamManagerDto
    /// </summary>
    /// <param name="manager">The team manager entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static FootballTeamManagerDto ToDto(FootballTeamManager manager)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));

        return new FootballTeamManagerDto(
            manager.Id,
            manager.PersonId,
            null!, // TODO: Add Person navigation property to FootballTeamManager entity or load Person separately
            manager.TeamId,
            manager.IsActive
        );
    }

    /// <summary>
    /// Maps a collection of FootballTeamManager entities to FootballTeamManagerDtos
    /// </summary>
    /// <param name="managers">The team manager entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when managers is null</exception>
    public static IEnumerable<FootballTeamManagerDto> ToDtos(IEnumerable<FootballTeamManager> managers)
    {
        if (managers == null)
            throw new ArgumentNullException(nameof(managers));

        return managers.Select(manager => ToDto(manager));
    }

    /// <summary>
    /// Creates a new FootballTeamManager entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team manager entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FootballTeamManager ToEntity(CreateFootballTeamManagerCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FootballTeamManager(
            command.PersonId,
            command.TeamId);
    }

    /// <summary>
    /// Updates a FootballTeamManager entity from an update command
    /// </summary>
    /// <param name="manager">The team manager entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when manager or command is null</exception>
    public static void UpdateFromCommand(FootballTeamManager manager, UpdateFootballTeamManagerCommand command)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        manager.UpdateActiveStatus(command.IsActive);
    }
} 
