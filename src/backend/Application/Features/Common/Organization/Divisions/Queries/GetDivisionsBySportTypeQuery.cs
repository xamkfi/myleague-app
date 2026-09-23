using MediatR;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;
using System.Collections.Generic;
using Domain.Enums.Common;

namespace Application.Features.Common.Organization.Divisions.Queries;

/// <summary>
/// Query for retrieving divisions by sport type
/// </summary>
public record GetDivisionsBySportTypeQuery(
    SportsCategory SportType, 
    bool ActiveOnly = false) : IRequest<Result<IEnumerable<DivisionDto>>>;
