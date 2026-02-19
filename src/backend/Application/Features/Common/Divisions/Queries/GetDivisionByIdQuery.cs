using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Queries.Common;

/// <summary>
/// Query for retrieving a division by ID
/// </summary>
public record GetDivisionByIdQuery(Guid Id) : IRequest<Result<DivisionDto>>; 