using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.Common;

/// <summary>
/// Query for retrieving divisions by sport type
/// </summary>
public record GetDivisionsBySportTypeQuery(
    string SportType, 
    bool ActiveOnly = false) : IRequest<Result<IEnumerable<DivisionDto>>>; 