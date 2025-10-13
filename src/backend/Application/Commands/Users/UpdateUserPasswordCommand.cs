using System;
using Application.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for updating user password
/// </summary>
public record UpdateUserPasswordCommand(
    Guid Id,
    string CurrentPassword,
    string NewPassword) : IRequest<Result<bool>>; 