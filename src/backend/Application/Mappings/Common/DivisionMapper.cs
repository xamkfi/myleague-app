using Application.Commands.Common;
using Application.DTOs.Common;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Common;

/// <summary>
/// Mapper class for Division entity and related DTOs
/// </summary>
public static class DivisionMapper
{
    /// <summary>
    /// Maps a Division entity to a DivisionDto
    /// </summary>
    /// <param name="division">The Division entity to map</param>
    /// <returns>A DivisionDto representing the Division entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if division is null</exception>
    public static DivisionDto ToDto(Division division)
    {
        if (division == null)
            throw new ArgumentNullException(nameof(division));

        return new DivisionDto(
            division.Id,
            division.Name,
            division.Description,
            division.Level,
            division.SportType.ToString(),
            division.IsActive,
            division.CreatedDate
        );
    }

    /// <summary>
    /// Maps a collection of Division entities to a collection of DivisionDtos
    /// </summary>
    /// <param name="divisions">The collection of Division entities to map</param>
    /// <returns>A collection of DivisionDtos</returns>
    /// <exception cref="ArgumentNullException">Thrown if divisions is null</exception>
    public static IEnumerable<DivisionDto> ToDtos(IEnumerable<Division> divisions)
    {
        if (divisions == null)
            throw new ArgumentNullException(nameof(divisions));

        return divisions.Select(division => ToDto(division));
    }

    /// <summary>
    /// Maps a CreateDivisionCommand to a Division entity
    /// </summary>
    /// <param name="command">The CreateDivisionCommand to map</param>
    /// <returns>A new Division entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
    public static Division ToEntity(CreateDivisionCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new Division(
            command.Name,
            command.Description,
            command.Level,
            command.SportType
        );
    }

    /// <summary>
    /// Updates a Division entity with values from an UpdateDivisionCommand
    /// </summary>
    /// <param name="division">The Division entity to update</param>
    /// <param name="command">The UpdateDivisionCommand containing updated values</param>
    /// <exception cref="ArgumentNullException">Thrown if division or command is null</exception>
    public static void UpdateFromCommand(Division division, UpdateDivisionCommand command)
    {
        if (division == null)
            throw new ArgumentNullException(nameof(division));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        division.UpdateDetails(command.Name, command.Description, command.Level);
    }
} 