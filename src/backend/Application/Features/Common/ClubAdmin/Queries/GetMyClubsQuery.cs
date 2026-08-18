using Application.Common;
using Application.Features.Common.ClubAdmin.DTOs;
using MediatR;

namespace Application.Features.Common.ClubAdmin.Queries;

/// <summary>
/// Query for retrieving all clubs (with their teams in both sports) that the given person
/// actively manages.
/// </summary>
/// <param name="PersonId">The person ID of the club admin</param>
public record GetMyClubsQuery(Guid PersonId) : IRequest<Result<IEnumerable<ClubAdminClubDto>>>;
