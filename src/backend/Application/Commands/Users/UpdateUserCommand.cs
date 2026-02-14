using Application.Common;
using Application.DTOs.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for updating an existing user
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string Email,
    UserRole Role) : IRequest<Result<UserDto>>;
