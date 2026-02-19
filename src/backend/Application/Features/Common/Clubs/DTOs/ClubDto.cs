using System;
using System.Collections.Generic;

namespace Application.Features.Common.Clubs.DTOs;

/// <summary>
/// Data Transfer Object for Club entity
/// </summary>
public record ClubDto(
    Guid Id,
    string Name,
    DateTime FoundingDate,
    string City,
    string Country,
    string WebsiteUrl,
    string LogoUrl,
    string ContactEmail);
