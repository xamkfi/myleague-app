using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
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

        return new FloorballPlayerDto
        {
            Id = player.Id,
            PersonId = player.PersonId,
            Position = player.Position,
            CareerGoals = player.CareerGoals,
            CareerAssists = player.CareerAssists,
            IsActive = player.IsActive,
            CreatedAt = player.CreatedAt.ToUniversalTime(),
            UpdatedAt = player.UpdatedAt?.ToUniversalTime()
        };
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

        return new FloorballPlayer(command.PersonId, command.Position);
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

        player.UpdatePosition(command.Position);
        player.UpdateActiveStatus(command.IsActive);
    }

    /// <summary>
    /// Updates a player's statistics
    /// </summary>
    /// <param name="player">The player entity to update</param>
    /// <param name="command">The update statistics command</param>
    /// <exception cref="ArgumentNullException">Thrown when player or command is null</exception>
    public static void UpdateStatistics(FloorballPlayer player, UpdateFloorballPlayerStatisticsCommand command)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Note: The entity has RecordGoal() and RecordAssist() methods
        // We should call these methods for each goal and assist difference
        int goalDifference = command.Goals - player.CareerGoals;
        int assistDifference = command.Assists - player.CareerAssists;

        for (int i = 0; i < goalDifference; i++)
        {
            player.RecordGoal();
        }

        for (int i = 0; i < assistDifference; i++)
        {
            player.RecordAssist();
        }
    }
} 