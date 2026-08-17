using Application.Common;
using MediatR;

namespace Application.Features.Common.Clubs.Commands;

/// <summary>
/// Command for replacing the set of club admins of a club. Users in the list get an active
/// club manager link; existing links for users not in the list are deactivated.
/// </summary>
/// <param name="ClubId">The club ID</param>
/// <param name="UserIds">The user IDs that should administer the club</param>
public record SetClubAdminsCommand(
    Guid ClubId,
    IReadOnlyList<Guid> UserIds) : IRequest<Result<bool>>;
