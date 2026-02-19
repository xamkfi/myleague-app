using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query for getting all users
/// </summary>
public record GetAllUsersQuery() : IRequest<Result<IEnumerable<UserDto>>>; 