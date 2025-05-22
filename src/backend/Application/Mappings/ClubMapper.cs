using Application.DTOs.Common;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings;

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

        return new ClubDto
        {
            Id = club.Id,
            Name = club.Name,
            FoundingDate = club.FoundingDate,
            City = club.City,
            Country = club.Country,
            WebsiteUrl = club.WebsiteUrl?.ToString() ?? string.Empty,
            LogoUrl = club.LogoUrl?.ToString() ?? string.Empty,
            ContactEmail = club.ContactEmail
        };
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
    /// Maps a CreateClubRequest to a Club entity
    /// </summary>
    /// <param name="request">The CreateClubRequest to map</param>
    /// <returns>A new Club entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if request is null</exception>
    public static Club ToEntity(CreateClubRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        Uri? websiteUri = !string.IsNullOrEmpty(request.WebsiteUrl) ? new Uri(request.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(request.LogoUrl) ? new Uri(request.LogoUrl) : null;

        return new Club(
            request.Name,
            request.City,
            request.Country,
            request.FoundingDate,
            websiteUri,
            logoUri,
            request.ContactEmail
        );
    }

    /// <summary>
    /// Updates a Club entity with values from an UpdateClubRequest
    /// </summary>
    /// <param name="club">The Club entity to update</param>
    /// <param name="request">The UpdateClubRequest containing updated values</param>
    /// <exception cref="ArgumentNullException">Thrown if club or request is null</exception>
    public static void UpdateFromRequest(Club club, UpdateClubRequest request)
    {
        if (club == null)
            throw new ArgumentNullException(nameof(club));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Update basic info
        club.UpdateBasicInfo(request.Name, request.City, request.Country);

        // Update online presence
        Uri? websiteUri = !string.IsNullOrEmpty(request.WebsiteUrl) ? new Uri(request.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(request.LogoUrl) ? new Uri(request.LogoUrl) : null;
        club.UpdateOnlinePresence(websiteUri, logoUri, request.ContactEmail);
    }
} 
