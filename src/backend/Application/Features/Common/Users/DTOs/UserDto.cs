using System;
using Application.Features.Common.Persons.DTOs;
using Domain.Enums.Common;

namespace Application.Features.Common.Users.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    Guid PersonId,
    UserRole Role,
    bool IsActive,
    bool IsEmailVerified,
    DateTime? LastLoginAt,
    PersonDto Person);
