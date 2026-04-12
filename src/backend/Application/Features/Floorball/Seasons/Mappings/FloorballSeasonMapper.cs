using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.Seasons.Mappings;

/// <summary>
/// Mapper for FloorballSeason entity
/// </summary>
public static class FloorballSeasonMapper
{
    /// <summary>
    /// Maps a collection of season divisions to DTOs.
    /// </summary>
    /// <param name="seasonDivisions">The season divisions to map</param>
    /// <returns>The mapped season division DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when seasonDivisions is null</exception>
    public static IReadOnlyCollection<FloorballSeasonDivisionDto> ToDivisionDtos(IEnumerable<FloorballCompetitionDivision> seasonDivisions)
    {
        if (seasonDivisions == null)
        {
            throw new ArgumentNullException(nameof(seasonDivisions));
        }

        return seasonDivisions
            .Select(sd => new FloorballSeasonDivisionDto(
                sd.DivisionId,
                sd.Teams.Count,
                sd.Teams.Select(t => t.TeamId).ToList().AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }


    /// <summary>
    /// Maps a FloorballSeason entity to a FloorballSeasonDto using pre-mapped season division DTOs.
    /// </summary>
    /// <param name="season">The season entity to map</param>
    /// <param name="seasonDivisions">Season division DTOs for the season</param>
    /// <param name="clubs">Dictionary of clubs keyed by club ID (optional)</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when season or seasonDivisions is null</exception>
    public static FloorballSeasonDto ToDto(
        FloorballCompetition season,
        IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisions,
        Dictionary<Guid, Club>? clubs = null,
        IEnumerable<FloorballTeam>? seasonTeams = null)
    {
        if (season == null)
        {
            throw new ArgumentNullException(nameof(season));
        }

        if (seasonDivisions == null)
        {
            throw new ArgumentNullException(nameof(seasonDivisions));
        }

        IEnumerable<FloorballTeam> teamsToMap = seasonTeams ?? season.Teams;

        FloorballMatchRulesDto matchRulesDto = new FloorballMatchRulesDto(
            season.MatchRules.NumberOfPeriods,
            season.MatchRules.PeriodDurationMinutes,
            season.MatchRules.AllowOvertime,
            season.MatchRules.OvertimeDurationMinutes,
            season.MatchRules.AllowShootout);

        return new FloorballSeasonDto(
            season.Id,
            season.Name,
            season.StartDate.ToUniversalTime(),
            season.EndDate.ToUniversalTime(),
            season.IsActive,
            season.IsCompleted,
            seasonDivisions,
            FloorballTeamMapper.ToDtos(teamsToMap, clubs, new Dictionary<Guid, Person>()).ToList().AsReadOnly(),
            FloorballMatchMapper.ToDtos(season.Matches).ToList().AsReadOnly(),
            matchRulesDto
        );
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

        FloorballMatchRules matchRules = new FloorballMatchRules(
            command.NumberOfPeriods,
            command.PeriodDurationMinutes,
            command.AllowOvertime,
            command.OvertimeDurationMinutes,
            command.AllowShootout);

        return new FloorballSeason(
         command.Name,
         startDateUtc,
         endDateUtc,
         matchRules
     );
    }

    /// <summary>
    /// Updates a FloorballSeason entity from an update command
    /// </summary>
    /// <param name="season">The season entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when season or command is null</exception>
    public static void UpdateFromCommand(FloorballCompetition season, UpdateFloorballSeasonCommand command)
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

        // Update match rules
        FloorballMatchRules matchRules = new FloorballMatchRules(
            command.NumberOfPeriods,
            command.PeriodDurationMinutes,
            command.AllowOvertime,
            command.OvertimeDurationMinutes,
            command.AllowShootout);
        season.UpdateMatchRules(matchRules);
    }
} 
