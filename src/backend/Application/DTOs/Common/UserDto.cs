using System;

namespace Application.DTOs.Common;

/// <summary>
/// Data transfer object for user information
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    Guid PersonId,
    bool IsActive,
    DateTime? LastLoginAt,
    PersonDto Person);
