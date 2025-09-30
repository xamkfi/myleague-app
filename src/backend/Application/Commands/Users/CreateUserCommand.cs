using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for creating a new user
/// </summary>
public record CreateUserCommand(
    string Username,
    string Password) : IRequest<Result<UserDto>>; 
