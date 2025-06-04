using Application.Commands.Floorball.Referee;
using Application.DTOs.Floorball;
using Application.Mappings.Common;
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

        return new FloorballRefereeDto(
            referee.Id,
            referee.PersonId,
            PersonMapper.ToDto(referee.Person),
            referee.IsActive,
            referee.LicenseIssueDate?.ToUniversalTime(),
            referee.LicenseExpiryDate?.ToUniversalTime(),
            referee.MatchesOfficiated
        );
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
            command.LicenseIssueDate.ToUniversalTime(),
            command.LicenseExpiryDate.ToUniversalTime());
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

        // Update active status using the existing method
        referee.UpdateActiveStatus(command.IsActive);
        
        // Update license expiry date if provided and different from current
        if (command.LicenseExpiryDate.HasValue && command.LicenseExpiryDate.Value != referee.LicenseExpiryDate)
        {
            referee.UpdateLicenseExpiry(command.LicenseExpiryDate.Value);
        }
        
        // Note: LicenseIssueDate and MatchesOfficiated cannot be updated directly
        // as the entity doesn't expose methods for these operations.
        // This follows domain-driven design principles where the issue date
        // should not be changed after creation, and matches are recorded individually.
    }
} 
