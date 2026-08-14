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
/// <paramref name="TeamAssignments"/> is only used when <paramref name="Role"/> is
/// <see cref="UserRole.TeamLeader"/>: for each assignment an active team manager link is
/// created (or reactivated) so the new team leader can manage those teams.
/// </remarks>
public record CreateUserCommand(
    string Email,
    Guid PersonId,
    UserRole Role = UserRole.ClubAdmin,
    IReadOnlyList<TeamAssignmentDto>? TeamAssignments = null) : IRequest<Result<UserDto>>;
