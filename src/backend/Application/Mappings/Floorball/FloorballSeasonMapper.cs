using Application.Commands.Floorball.Season;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballSeason entity
/// </summary>
public static class FloorballSeasonMapper
{
    /// <summary>
    /// Maps a FloorballSeason entity to a FloorballSeasonDto (async version that loads season divisions)
    /// </summary>
    /// <param name="season">The season entity to map</param>
    /// <param name="seasonDivisionRepository">The season division repository to load divisions</param>
    /// <param name="clubs">Dictionary of clubs keyed by club ID (optional)</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when season or seasonDivisionRepository is null</exception>
    public static async Task<FloorballSeasonDto> ToDtoAsync(
        FloorballSeason season, 
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        Dictionary<Guid, Club>? clubs = null)
    {
        if (season == null)
            throw new ArgumentNullException(nameof(season));
        if (seasonDivisionRepository == null)
            throw new ArgumentNullException(nameof(seasonDivisionRepository));

        // Load season divisions
        IEnumerable<FloorballSeasonDivision> seasonDivisions = await seasonDivisionRepository.GetSeasonDivisionsAsync(season.Id);
        IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = seasonDivisions.Select(sd => new FloorballSeasonDivisionDto(
            sd.DivisionId,
            sd.Teams.Count
        )).ToList().AsReadOnly();

        return new FloorballSeasonDto(
            season.Id,
            season.Name,
            season.StartDate.ToUniversalTime(),
            season.EndDate.ToUniversalTime(),
            season.IsActive,
            season.IsCompleted,
            seasonDivisionDtos,
            FloorballTeamMapper.ToDtos(season.Teams, clubs, new Dictionary<Guid, Person>()).ToList().AsReadOnly(),
            FloorballMatchMapper.ToDtos(season.Matches).ToList().AsReadOnly()
        );
    }


    /// <summary>
    /// Maps a collection of FloorballSeason entities to FloorballSeasonDtos (async version that loads season divisions)
    /// </summary>
    /// <param name="seasons">The season entities to map</param>
    /// <param name="seasonDivisionRepository">The season division repository to load divisions</param>
    /// <param name="clubs">Dictionary of clubs keyed by club ID (optional)</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when seasons or seasonDivisionRepository is null</exception>
    public static async Task<IEnumerable<FloorballSeasonDto>> ToDtosAsync(
        IEnumerable<FloorballSeason> seasons, 
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        Dictionary<Guid, Club>? clubs = null)
    {
        if (seasons == null)
            throw new ArgumentNullException(nameof(seasons));
        if (seasonDivisionRepository == null)
            throw new ArgumentNullException(nameof(seasonDivisionRepository));

        List<FloorballSeason> seasonList = seasons.ToList();
        List<FloorballSeasonDto> dtos = new List<FloorballSeasonDto>();

        foreach (FloorballSeason season in seasonList)
        {
            FloorballSeasonDto dto = await ToDtoAsync(season, seasonDivisionRepository, clubs);
            dtos.Add(dto);
        }

        return dtos;
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

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime startDateUtc = command.StartDate.Kind switch
        {
            DateTimeKind.Utc => command.StartDate,
            DateTimeKind.Local => command.StartDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.StartDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.StartDate, DateTimeKind.Utc)
        };

        DateTime endDateUtc = command.EndDate.Kind switch
        {
            DateTimeKind.Utc => command.EndDate,
            DateTimeKind.Local => command.EndDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.EndDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.EndDate, DateTimeKind.Utc)
        };

        return new FloorballSeason(
         command.Name,
         startDateUtc,
         endDateUtc
     );
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

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime startDateUtc = command.StartDate.Kind switch
        {
            DateTimeKind.Utc => command.StartDate,
            DateTimeKind.Local => command.StartDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.StartDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.StartDate, DateTimeKind.Utc)
        };

        DateTime endDateUtc = command.EndDate.Kind switch
        {
            DateTimeKind.Utc => command.EndDate,
            DateTimeKind.Local => command.EndDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.EndDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.EndDate, DateTimeKind.Utc)
        };

        // Use the entity's UpdateDetails method to update name and date range
        season.UpdateDetails(
            command.Name,
            startDateUtc,
            endDateUtc
        );
    }
} 
