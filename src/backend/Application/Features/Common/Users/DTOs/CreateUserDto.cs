using System;

namespace Application.DTOs.Common;

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
public record CreateUserDto(
    string Username,
    string Password,
    Guid PersonId); 