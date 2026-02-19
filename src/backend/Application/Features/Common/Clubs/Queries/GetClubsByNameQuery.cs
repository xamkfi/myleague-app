using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using MediatR;

namespace Application.Features.Common.Clubs.Queries;

/// <summary>
/// Query for retrieving clubs by name search
/// </summary>
public record GetClubsByNameQuery(string name) : IRequest<Result<IEnumerable<ClubDto>>>;
