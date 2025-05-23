using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.Clubs;

/// <summary>
/// Query for retrieving all clubs
/// </summary>
public record GetAllClubsQuery() : IRequest<Result<IEnumerable<ClubDto>>>; 