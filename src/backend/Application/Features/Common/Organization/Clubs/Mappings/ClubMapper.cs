using Application.Features.Common.Organization.Clubs.Commands;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Common.Organization.Clubs.Mappings;

/// <summary>
/// Mapper class for Club entity and related DTOs
/// </summary>
public static class ClubMapper
{
    /// <summary>
    /// Maps a Club entity to a ClubDto
    /// </summary>
    /// <param name="club">The Club entity to map</param>
    /// <returns>A ClubDto representing the Club entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if club is null</exception>
    public static ClubDto ToDto(Club club)
    {
        if (club == null)
            throw new ArgumentNullException(nameof(club));

        return new ClubDto(
            club.Id,
            club.Name,
            club.FoundingDate,
            club.City,
            club.Country,
            ToPublicUrl(club.WebsiteUrl),
            ToPublicUrl(club.LogoUrl),
            ToPublicEmail(club.ContactEmail)
        );
    }

    /// <summary>
    /// Maps a collection of Club entities to a collection of ClubDtos
    /// </summary>
    /// <param name="clubs">The collection of Club entities to map</param>
    /// <returns>A collection of ClubDtos</returns>
    /// <exception cref="ArgumentNullException">Thrown if clubs is null</exception>
    public static IEnumerable<ClubDto> ToDtos(IEnumerable<Club> clubs)
    {
        if (clubs == null)
            throw new ArgumentNullException(nameof(clubs));

        return clubs.Select(club => ToDto(club));
    }

    /// <summary>
    /// Maps a CreateClubCommand to a Club entity
    /// </summary>
    /// <param name="command">The CreateClubCommand to map</param>
    /// <returns>A new Club entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
    public static Club ToEntity(CreateClubCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Uri? websiteUri = !string.IsNullOrEmpty(command.WebsiteUrl) ? new Uri(command.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;

        return new Club(
            command.Name,
            command.City,
            command.Country,
            ToUtc(command.FoundingDate),
            websiteUri,
            logoUri,
            command.ContactEmail
        );
    }

    /// <summary>
    /// Updates a Club entity with values from an UpdateClubCommand
    /// </summary>
    /// <param name="club">The Club entity to update</param>
    /// <param name="command">The UpdateClubCommand containing updated values</param>
    /// <exception cref="ArgumentNullException">Thrown if club or command is null</exception>
    public static void UpdateFromCommand(Club club, UpdateClubCommand command)
    {
        if (club == null)
            throw new ArgumentNullException(nameof(club));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Update basic info
        club.UpdateBasicInfo(command.Name, command.City, command.Country);

        club.UpdateFoundingDate(ToUtc(command.FoundingDate));

        // Update online presence
        Uri? websiteUri = !string.IsNullOrEmpty(command.WebsiteUrl) ? new Uri(command.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;
        club.UpdateOnlinePresence(websiteUri, logoUri, command.ContactEmail);
    }

    private static string ToPublicUrl(Uri? uri)
    {
        if (uri is null)
        {
            return string.Empty;
        }

        string host = uri.Host;
        if (host.Equals("example.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".example.com", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return uri.ToString();
    }

    private static string ToPublicEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        string trimmed = email.Trim();
        if (trimmed.Equals("contact@example.com", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return trimmed;
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        DateTime foundingDate = value.Value;
        return foundingDate.Kind switch
        {
            DateTimeKind.Utc => foundingDate,
            DateTimeKind.Local => foundingDate.ToUniversalTime(),
            _ => DateTime.SpecifyKind(foundingDate, DateTimeKind.Utc)
        };
    }
} 
