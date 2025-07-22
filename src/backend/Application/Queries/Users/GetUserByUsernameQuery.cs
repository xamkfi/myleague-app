using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query for getting a user by username
/// </summary>
public record GetUserByUsernameQuery(string Username) : IRequest<Result<UserDto>>; 