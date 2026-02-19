using System;

namespace Application.Features.Common.Users.DTOs;

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
public record CreateUserDto(
    string Username,
    string Password,
    Guid PersonId); 
