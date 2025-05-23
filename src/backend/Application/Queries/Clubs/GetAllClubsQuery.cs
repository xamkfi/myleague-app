using MediatR;
using Application.DTOs.Common;
using System.Collections.Generic;

namespace Application.Queries.Clubs;

/// <summary>
/// Query for retrieving all clubs
/// </summary>
public record GetAllClubsQuery() : IRequest<IEnumerable<ClubDto>>; 