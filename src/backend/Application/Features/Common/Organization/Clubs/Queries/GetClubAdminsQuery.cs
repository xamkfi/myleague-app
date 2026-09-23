using Application.Common;
using Application.Features.Common.Organization.Clubs.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.Clubs.Queries;

/// <summary>
/// Query for retrieving the active club admins of a club.
/// </summary>
/// <param name="ClubId">The club ID</param>
public record GetClubAdminsQuery(Guid ClubId) : IRequest<Result<IEnumerable<ClubAdminUserDto>>>;
