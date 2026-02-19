using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query for getting a user by ID
/// </summary>
public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>; 