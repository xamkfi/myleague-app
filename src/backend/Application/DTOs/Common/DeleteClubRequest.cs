using System;

namespace Application.DTOs.Common;

/// <summary>
/// Request DTO for deleting a club
/// </summary>
public record DeleteClubRequest
{
    /// <summary>
    /// Gets or sets the ID of the club to delete
    /// </summary>
    public Guid ClubId { get; init; }
} 