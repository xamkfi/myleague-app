using Application.Features.Football.Referees.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Domain.Entities.Football.Teams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Football.Referees.Mappings;

/// <summary>
/// Mapper for FootballReferee entity
/// </summary>
public static class FootballRefereeMapper
{
    /// <summary>
    /// Maps a FootballReferee entity to a FootballRefereeDto
    /// </summary>
    /// <param name="referee">The referee entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when referee is null</exception>
    public static FootballRefereeDto ToDto(FootballReferee referee)
    {
        if (referee == null)
            throw new ArgumentNullException(nameof(referee));

        return new FootballRefereeDto(
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
    /// Maps a collection of FootballReferee entities to FootballRefereeDtos
    /// </summary>
    /// <param name="referees">The referee entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when referees is null</exception>
    public static IEnumerable<FootballRefereeDto> ToDtos(IEnumerable<FootballReferee> referees)
    {
        if (referees == null)
            throw new ArgumentNullException(nameof(referees));

        return referees.Select(referee => ToDto(referee));
    }

    /// <summary>
    /// Creates a new FootballReferee entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new referee entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FootballReferee ToEntity(CreateFootballRefereeCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime licenseIssueDateUtc = command.LicenseIssueDate.Kind switch
        {
            DateTimeKind.Utc => command.LicenseIssueDate,
            DateTimeKind.Local => command.LicenseIssueDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.LicenseIssueDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.LicenseIssueDate, DateTimeKind.Utc)
        };

        DateTime licenseExpiryDateUtc = command.LicenseExpiryDate.Kind switch
        {
            DateTimeKind.Utc => command.LicenseExpiryDate,
            DateTimeKind.Local => command.LicenseExpiryDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.LicenseExpiryDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.LicenseExpiryDate, DateTimeKind.Utc)
        };

        return new FootballReferee(
            command.PersonId,
            licenseIssueDateUtc,
            licenseExpiryDateUtc);
    }

    /// <summary>
    /// Updates a FootballReferee entity from an update command
    /// </summary>
    /// <param name="referee">The referee entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when referee or command is null</exception>
    public static void UpdateFromCommand(FootballReferee referee, UpdateFootballRefereeCommand command)
    {
        if (referee == null)
            throw new ArgumentNullException(nameof(referee));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Update active status using the existing method
        referee.UpdateActiveStatus(command.IsActive);
        // Update matches officiated
        referee.UpdateMatchesOfficiated(command.MatchesOfficiated);
        
        // Update license expiry date if provided and different from current
        if (command.LicenseExpiryDate.HasValue)
        {
            // Ensure DateTime is in UTC
            DateTime licenseExpiryDateUtc = command.LicenseExpiryDate.Value.Kind switch
            {
                DateTimeKind.Utc => command.LicenseExpiryDate.Value,
                DateTimeKind.Local => command.LicenseExpiryDate.Value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(command.LicenseExpiryDate.Value, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(command.LicenseExpiryDate.Value, DateTimeKind.Utc)
            };

            if (licenseExpiryDateUtc != referee.LicenseExpiryDate)
            {
                referee.UpdateLicenseExpiry(licenseExpiryDateUtc);
            }
        }
        
        // Note: License issue date cannot be updated after creation as the entity doesn't support it
        // This is by design - license issue dates should be immutable once set
    }
}
