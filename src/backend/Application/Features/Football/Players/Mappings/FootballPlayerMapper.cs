using Application.Features.Football.Players.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.ValueObjects.Football;
using Domain.Enums.Football;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Football.Players.Mappings;

/// <summary>
/// Mapper for FootballPlayer entity
/// </summary>
public static class FootballPlayerMapper
{
    /// <summary>
    /// Maps a FootballPlayer entity to a FootballPlayerDto
    /// </summary>
    /// <param name="player">The player entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null</exception>
    public static FootballPlayerDto ToDto(FootballPlayer player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        // TODO: In a complete implementation, PersonDto should be loaded from PersonRepository
        // Handle case where Person navigation property is null (ignored in EF config)
        PersonDto placeholderPerson = player.Person != null 
            ? new PersonDto(
                player.PersonId,
                player.Person.FirstName,
                player.Person.LastName,
                player.Person.BirthDate,
                player.Person.FullName,
                player.Person.role,
                player.Person.IsRegistered,
                player.Person.Address,
                player.Person.ContactInfo
            )
            : new PersonDto(
                player.PersonId,
                "Unknown",
                "Player", 
                null,
                "Unknown Player",
                Domain.Enums.Common.PersonRole.User,
                false,
                null,
                null
            );

        return new FootballPlayerDto(
            player.Id,
            player.PersonId,
            placeholderPerson,
            player.IsActive,
            player.Position.PrimaryPosition,
            player.CareerGoals,
            player.CareerAssists,
            null); // Team information not available in this mapping
    }

    /// <summary>
    /// Maps a collection of FootballPlayer entities to FootballPlayerDtos
    /// </summary>
    /// <param name="players">The player entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when players is null</exception>
    public static IEnumerable<FootballPlayerDto> ToDtos(IEnumerable<FootballPlayer> players)
    {
        if (players == null)
            throw new ArgumentNullException(nameof(players));

        return players.Select(player => ToDto(player));
    }

    /// <summary>
    /// Creates a new FootballPlayer entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new player entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FootballPlayer ToEntity(CreateFootballPlayerCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Create with default position (None) - position will be set when player is added to a team
        FootballPositionPreference position = new FootballPositionPreference(FootballPosition.None);
        return new FootballPlayer(command.PersonId, position);
    }

    /// <summary>
    /// Updates a FootballPlayer entity from an update command
    /// </summary>
    /// <param name="player">The player entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when player or command is null</exception>
    public static void UpdateFromCommand(FootballPlayer player, UpdateFootballPlayerCommand command)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Only update active status - position is now managed at team level
        player.UpdateActiveStatus(command.IsActive);
    }

    // TODO: Implement UpdateFootballPlayerStatisticsCommand
} 
