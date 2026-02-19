using Application.Common;
using Application.DTOs.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command for creating a new user
/// </summary>
public record CreateUserCommand(
    string Email,
    Guid PersonId,
    UserRole Role = UserRole.ClubAdmin) : IRequest<Result<UserDto>>;
