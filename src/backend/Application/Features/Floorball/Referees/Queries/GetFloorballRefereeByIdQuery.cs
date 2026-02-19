using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Referee
{
    /// <summary>
    /// Query for retrieving a single floorball referee by ID
    /// </summary>
    public record GetFloorballRefereeByIdQuery(Guid Id) : IRequest<Result<FloorballRefereeDto>>;
} 