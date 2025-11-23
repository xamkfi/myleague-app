using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.Clubs;

/// <summary>
/// Query for retrieving all clubs
/// </summary>
public record GetAllClubsQuery() : IRequest<Result<IEnumerable<ClubDto>>>;

/// <summary>
/// Query for retrieving paginated clubs
/// </summary>
public record GetClubsPagedQuery(int Page, int PageSize) : IRequest<Result<Domain.Common.PagedResult<ClubDto>>>; 