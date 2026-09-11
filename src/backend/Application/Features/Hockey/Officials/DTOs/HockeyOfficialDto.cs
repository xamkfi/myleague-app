namespace Application.Features.Hockey.Officials.DTOs;

/// <summary>
/// Data transfer object for a hockey official profile.
/// </summary>
public record HockeyOfficialDto(
    Guid Id,
    Guid PersonId,
    string OfficialRole,
    string? OfficialNumber,
    bool IsActive,
    DateTime? LicenseIssueDate,
    DateTime? LicenseExpiryDate,
    int MatchesOfficiated);
