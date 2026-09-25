using Application.Features.Common.Shared.DTOs;

namespace Application.Features.Common.Organization.DataSubjectRights.DTOs;

/// <summary>
/// Account fields included in a data-subject copy. Login codes and tokens are omitted.
/// </summary>
public record DataSubjectAccountDto(
    Guid UserId,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt);

/// <summary>
/// Data the person supplied, in a form that can be moved to another system.
/// </summary>
public record DataSubjectPortableDto(
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    AddressDto? Address,
    ContactInfoDto? ContactInfo,
    string? AccountEmail);

/// <summary>
/// Copy of personal data held for one person, including restriction and retention state.
/// </summary>
public record DataSubjectCopyDto(
    Guid PersonId,
    bool IsProcessed,
    string FirstName,
    string LastName,
    string FullName,
    DateTime? BirthDate,
    string Role,
    bool IsRegistered,
    AddressDto? Address,
    ContactInfoDto? ContactInfo,
    bool IsProcessingRestricted,
    bool HasObjectedToLegitimateInterest,
    DateTime? AnonymizedAt,
    string? RetentionReason,
    DataSubjectAccountDto? Account,
    DataSubjectPortableDto PortableData);
