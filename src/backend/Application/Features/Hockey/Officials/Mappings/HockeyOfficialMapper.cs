using Application.Features.Hockey.Officials.DTOs;
using Domain.Entities.Hockey.Teams;

namespace Application.Features.Hockey.Officials.Mappings;

/// <summary>
/// Maps hockey official entities to DTOs.
/// </summary>
public static class HockeyOfficialMapper
{
    public static HockeyOfficialDto ToDto(HockeyOfficial official) =>
        new(
            official.Id,
            official.PersonId,
            official.OfficialRole.ToString(),
            official.OfficialNumber,
            official.IsActive,
            official.LicenseIssueDate,
            official.LicenseExpiryDate,
            official.MatchesOfficiated);
}
