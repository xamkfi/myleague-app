using System;
using Application.Common;
using MediatR;

namespace Application.Features.Common.Users.Commands;

/// <summary>
/// Command for deleting a user
/// </summary>
public record DeleteUserCommand(Guid Id) : IRequest<Result<bool>>; 
