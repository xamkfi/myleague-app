namespace Application.Features.Common.Organization.PlayerLicences.DTOs;

/// <summary>
/// A paid roster licence on a competition that is still active.
/// </summary>
public record ActivePlayerLicenceDto(
    Guid TeamId,
    string TeamName,
    Guid? CompetitionId,
    string? CompetitionName);
