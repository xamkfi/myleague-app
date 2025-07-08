using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query for getting a user by person ID
/// </summary>
public record GetUserByPersonIdQuery(Guid PersonId) : IRequest<Result<UserDto>>; 