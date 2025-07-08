using System;
using Application.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for deleting a user
/// </summary>
public record DeleteUserCommand(Guid Id) : IRequest<Result<bool>>; 