using System;

namespace Application.DTOs.Common;

/// <summary>
/// Data transfer object for updating an existing user
/// </summary>
public record UpdateUserDto(
    string Username,
    string? Password = null); 