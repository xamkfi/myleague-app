using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballTeam entity
/// </summary>
public static class FloorballTeamMapper
{
    /// <summary>
    /// Maps a FloorballTeam entity to a FloorballTeamDto
    /// </summary>
    /// <param name="team">The team entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when team is null</exception>
    public static FloorballTeamDto ToDto(FloorballTeam team)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        return new FloorballTeamDto
        {
            Id = team.Id,
            Name = team.Name,
            City = team.City,
            HomeVenue = team.HomeVenue,
            Founded = team.Founded,
            Website = team.Website,
            ContactEmail = team.ContactEmail,
            ContactPhone = team.ContactPhone,
            SeasonId = team.SeasonId,
            CreatedAt = team.CreatedAt.ToUniversalTime(),
            UpdatedAt = team.UpdatedAt?.ToUniversalTime(),
            IsActive = team.IsActive
        };
    }

    /// <summary>
    /// Maps a collection of FloorballTeam entities to FloorballTeamDtos
    /// </summary>
    /// <param name="teams">The team entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when teams is null</exception>
    public static IEnumerable<FloorballTeamDto> ToDtos(IEnumerable<FloorballTeam> teams)
    {
        if (teams == null)
            throw new ArgumentNullException(nameof(teams));

        return teams.Select(team => ToDto(team));
    }

    /// <summary>
    /// Creates a new FloorballTeam entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballTeam ToEntity(CreateFloorballTeamCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballTeam
        {
            Name = command.Name,
            City = command.City,
            HomeVenue = command.HomeVenue,
            Founded = command.Founded,
            Website = command.Website,
            ContactEmail = command.ContactEmail,
            ContactPhone = command.ContactPhone,
            SeasonId = command.SeasonId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates a FloorballTeam entity from an update command
    /// </summary>
    /// <param name="team">The team entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when team or command is null</exception>
    public static void UpdateFromCommand(FloorballTeam team, UpdateFloorballTeamCommand command)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        team.Name = command.Name;
        team.City = command.City;
        team.HomeVenue = command.HomeVenue;
        team.Founded = command.Founded;
        team.Website = command.Website;
        team.ContactEmail = command.ContactEmail;
        team.ContactPhone = command.ContactPhone;
        team.SeasonId = command.SeasonId;
        team.IsActive = command.IsActive;
        team.UpdatedAt = DateTime.UtcNow;
    }
} 