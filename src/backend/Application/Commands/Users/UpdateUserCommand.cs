using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for updating an existing user
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string Username,
    string? Password = null) : IRequest<Result<UserDto>>; 