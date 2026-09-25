using Application.Features.Common.Shared.DTOs;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for correcting personal data a data subject supplied.
/// </summary>
public record RectifyDataSubjectRequest(
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    AddressDto? Address,
    ContactInfoDto? ContactInfo,
    string? AccountEmail);

/// <summary>
/// Request model for restricting processing or recording an objection.
/// </summary>
public record SetDataSubjectFlagRequest(bool Enabled);
