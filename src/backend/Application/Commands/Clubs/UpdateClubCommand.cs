using System;

namespace Application.Commands.Clubs;

/// <summary>
/// Command for updating an existing club
/// </summary>
public record UpdateClubCommand(
    Guid ClubId,
    string Name,
    string City,
    string Country,
    DateTime FoundingDate,
    string WebsiteUrl = "",
    string LogoUrl = "",
    string ContactEmail = ""); 