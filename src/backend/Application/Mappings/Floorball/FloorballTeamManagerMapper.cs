using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

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

        return new FloorballTeamManagerDto
        {
            Id = manager.Id,
            PersonId = manager.PersonId,
            IsActive = manager.IsActive,
            PrimaryResponsibility = manager.PrimaryResponsibility,
            YearsOfExperience = manager.YearsOfExperience,
            CreatedAt = manager.CreatedAt.ToUniversalTime(),
            UpdatedAt = manager.UpdatedAt?.ToUniversalTime()
        };
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
            command.YearsOfExperience,
            command.PrimaryResponsibility);
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

        // Note: The entity needs to expose methods for updating these properties
        // For now, assuming these methods exist or will be added
        manager.UpdateActiveStatus(command.IsActive);
        manager.UpdateExperience(command.YearsOfExperience);
        manager.UpdateResponsibility(command.PrimaryResponsibility);
    }
} 