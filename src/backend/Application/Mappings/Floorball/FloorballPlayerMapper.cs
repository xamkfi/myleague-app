using Application.Commands.Floorball.Player;
using Application.DTOs.Floorball;
using Application.DTOs.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.ValueObjects.Floorball;
using Domain.Enums.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballPlayer entity
/// </summary>
public static class FloorballPlayerMapper
{
    /// <summary>
    /// Maps a FloorballPlayer entity to a FloorballPlayerDto
    /// </summary>
    /// <param name="player">The player entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null</exception>
    public static FloorballPlayerDto ToDto(FloorballPlayer player)
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
                player.Person.IsRegistered,
                player.Person.Address,
                player.Person.ContactInfo
            )
            : new PersonDto(
                player.PersonId,
                "Unknown",
                "Player", 
                DateTime.MinValue,
                "Unknown Player",
                false,
                null,
                null
            );

        return new FloorballPlayerDto(
            player.Id,
            player.PersonId,
            placeholderPerson,
            player.IsActive,
            player.Position.PrimaryPosition,
            player.CareerGoals,
            player.CareerAssists);
    }

    /// <summary>
    /// Maps a collection of FloorballPlayer entities to FloorballPlayerDtos
    /// </summary>
    /// <param name="players">The player entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when players is null</exception>
    public static IEnumerable<FloorballPlayerDto> ToDtos(IEnumerable<FloorballPlayer> players)
    {
        if (players == null)
            throw new ArgumentNullException(nameof(players));

        return players.Select(player => ToDto(player));
    }

    /// <summary>
    /// Creates a new FloorballPlayer entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new player entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballPlayer ToEntity(CreateFloorballPlayerCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Create with default position (None) - position will be set when player is added to a team
        Position position = new Position(FloorballPosition.None);
        return new FloorballPlayer(command.PersonId, position);
    }

    /// <summary>
    /// Updates a FloorballPlayer entity from an update command
    /// </summary>
    /// <param name="player">The player entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when player or command is null</exception>
    public static void UpdateFromCommand(FloorballPlayer player, UpdateFloorballPlayerCommand command)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Only update active status - position is now managed at team level
        player.UpdateActiveStatus(command.IsActive);
    }

    // TODO: Implement UpdateFloorballPlayerStatisticsCommand
} 
