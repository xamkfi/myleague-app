using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Referees.Queries
{
    /// <summary>
    /// Query for retrieving a single floorball referee by ID
    /// </summary>
    public record GetFloorballRefereeByIdQuery(Guid Id) : IRequest<Result<FloorballRefereeDto>>;
} 
