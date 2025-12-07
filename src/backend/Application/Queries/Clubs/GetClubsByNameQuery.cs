using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Clubs;

/// <summary>
/// Query for retrieving clubs by name search
/// </summary>
public record GetClubsByNameQuery(string name) : IRequest<Result<IEnumerable<ClubDto>>>;
