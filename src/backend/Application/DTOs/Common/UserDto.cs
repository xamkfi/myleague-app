using System;

namespace Application.DTOs.Common;

/// <summary>
/// Data transfer object for user information
/// </summary>
public record UserDto(
    Guid Id,
    string Username,
    Guid PersonId,
    PersonDto Person); 