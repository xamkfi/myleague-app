using Application.Common;
using MediatR;

namespace Application.Features.Common.Users.Commands;

/// <summary>
/// Command for deleting a user.
/// </summary>
/// <param name="Id">The user to delete.</param>
/// <param name="RequestedByUserId">The authenticated user performing the delete.</param>
public record DeleteUserCommand(Guid Id, Guid RequestedByUserId) : IRequest<Result<bool>>;
