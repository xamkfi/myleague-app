using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballReferee entity
/// </summary>
public static class FloorballRefereeMapper
{
    /// <summary>
    /// Maps a FloorballReferee entity to a FloorballRefereeDto
    /// </summary>
    /// <param name="referee">The referee entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when referee is null</exception>
    public static FloorballRefereeDto ToDto(FloorballReferee referee)
    {
        if (referee == null)
            throw new ArgumentNullException(nameof(referee));

        return new FloorballRefereeDto
        {
            Id = referee.Id,
            PersonId = referee.PersonId,
            IsActive = referee.IsActive,
            LicenseIssueDate = referee.LicenseIssueDate?.ToUniversalTime(),
            LicenseExpiryDate = referee.LicenseExpiryDate?.ToUniversalTime(),
            MatchesOfficiated = referee.MatchesOfficiated,
            CreatedAt = referee.CreatedAt.ToUniversalTime(),
            UpdatedAt = referee.UpdatedAt?.ToUniversalTime()
        };
    }

    /// <summary>
    /// Maps a collection of FloorballReferee entities to FloorballRefereeDtos
    /// </summary>
    /// <param name="referees">The referee entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when referees is null</exception>
    public static IEnumerable<FloorballRefereeDto> ToDtos(IEnumerable<FloorballReferee> referees)
    {
        if (referees == null)
            throw new ArgumentNullException(nameof(referees));

        return referees.Select(referee => ToDto(referee));
    }

    /// <summary>
    /// Creates a new FloorballReferee entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new referee entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballReferee ToEntity(CreateFloorballRefereeCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballReferee(
            command.PersonId,
            command.LicenseIssueDate?.ToUniversalTime(),
            command.LicenseExpiryDate?.ToUniversalTime());
    }

    /// <summary>
    /// Updates a FloorballReferee entity from an update command
    /// </summary>
    /// <param name="referee">The referee entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when referee or command is null</exception>
    public static void UpdateFromCommand(FloorballReferee referee, UpdateFloorballRefereeCommand command)
    {
        if (referee == null)
            throw new ArgumentNullException(nameof(referee));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Note: The entity needs to expose methods for updating these properties
        // For now, assuming these methods exist or will be added
        referee.UpdateActiveStatus(command.IsActive);
        referee.UpdateLicenseDates(
            command.LicenseIssueDate?.ToUniversalTime(),
            command.LicenseExpiryDate?.ToUniversalTime());
    }
} 