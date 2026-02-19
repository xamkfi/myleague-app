using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query for getting a user by email address
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;
