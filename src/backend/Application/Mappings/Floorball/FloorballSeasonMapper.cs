using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballSeason entity
/// </summary>
public static class FloorballSeasonMapper
{
    /// <summary>
    /// Maps a FloorballSeason entity to a FloorballSeasonDto
    /// </summary>
    /// <param name="season">The season entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when season is null</exception>
    public static FloorballSeasonDto ToDto(FloorballSeason season)
    {
        if (season == null)
            throw new ArgumentNullException(nameof(season));

        return new FloorballSeasonDto
        {
            Id = season.Id,
            Name = season.Name,
            StartDate = season.StartDate.ToUniversalTime(),
            EndDate = season.EndDate.ToUniversalTime(),
            Description = season.Description,
            Rules = season.Rules,
            CreatedAt = season.CreatedAt.ToUniversalTime(),
            UpdatedAt = season.UpdatedAt?.ToUniversalTime(),
            IsActive = season.IsActive
        };
    }

    /// <summary>
    /// Maps a collection of FloorballSeason entities to FloorballSeasonDtos
    /// </summary>
    /// <param name="seasons">The season entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when seasons is null</exception>
    public static IEnumerable<FloorballSeasonDto> ToDtos(IEnumerable<FloorballSeason> seasons)
    {
        if (seasons == null)
            throw new ArgumentNullException(nameof(seasons));

        return seasons.Select(season => ToDto(season));
    }

    /// <summary>
    /// Creates a new FloorballSeason entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new season entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballSeason ToEntity(CreateFloorballSeasonCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballSeason
        {
            Name = command.Name,
            StartDate = command.StartDate.ToUniversalTime(),
            EndDate = command.EndDate.ToUniversalTime(),
            Description = command.Description,
            Rules = command.Rules,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates a FloorballSeason entity from an update command
    /// </summary>
    /// <param name="season">The season entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when season or command is null</exception>
    public static void UpdateFromCommand(FloorballSeason season, UpdateFloorballSeasonCommand command)
    {
        if (season == null)
            throw new ArgumentNullException(nameof(season));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        season.Name = command.Name;
        season.StartDate = command.StartDate.ToUniversalTime();
        season.EndDate = command.EndDate.ToUniversalTime();
        season.Description = command.Description;
        season.Rules = command.Rules;
        season.IsActive = command.IsActive;
        season.UpdatedAt = DateTime.UtcNow;
    }
} 