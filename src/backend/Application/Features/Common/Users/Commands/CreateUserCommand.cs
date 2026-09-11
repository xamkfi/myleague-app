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
/// <remarks>
/// <paramref name="ClubAssignments"/> is only used when <paramref name="Role"/> is
/// <see cref="UserRole.ClubAdmin"/>: for each club an active club manager link is
/// created (or reactivated) so the new club admin can manage that club and its teams.
/// </remarks>
public record CreateUserCommand(
    string Email,
    Guid PersonId,
    UserRole Role = UserRole.ClubAdmin,
    IReadOnlyList<Guid>? ClubAssignments = null) : IRequest<Result<UserDto>>;
