using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.Common;

/// <summary>
/// Query for retrieving all divisions
/// </summary>
public record GetAllDivisionsQuery() : IRequest<Result<IEnumerable<DivisionDto>>>; 