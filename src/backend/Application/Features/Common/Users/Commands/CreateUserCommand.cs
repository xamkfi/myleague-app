using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Common.Users.Commands;

/// <summary>
/// Command for creating a new user
/// </summary>
public record CreateUserCommand(
    string Email,
    Guid PersonId,
    UserRole Role = UserRole.ClubAdmin) : IRequest<Result<UserDto>>;
