using System;

namespace Application.DTOs.Common;

/// <summary>
/// Request DTO for retrieving a club by its ID
/// </summary>
public record GetClubByIdRequest
{
    /// <summary>
    /// Gets or sets the ID of the club to retrieve
    /// </summary>
    public Guid ClubId { get; init; }
} 