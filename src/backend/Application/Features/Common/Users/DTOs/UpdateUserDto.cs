using System;

namespace Application.Features.Common.Users.DTOs;

/// <summary>
/// Data transfer object for updating an existing user
/// </summary>
public record UpdateUserDto(
    string Username,
    string? Password = null); 
