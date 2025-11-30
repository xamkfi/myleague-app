using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;
using Domain.Enums.Common;

namespace Application.Queries.Common;

/// <summary>
/// Query for retrieving divisions by sport type
/// </summary>
public record GetDivisionsBySportTypeQuery(
    SportsCategory SportType, 
    bool ActiveOnly = false) : IRequest<Result<IEnumerable<DivisionDto>>>;