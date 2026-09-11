using System;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Referees.Queries
{
    /// <summary>
    /// Query for retrieving a single football referee by ID
    /// </summary>
    public record GetFootballRefereeByIdQuery(Guid Id) : IRequest<Result<FootballRefereeDto>>;
} 
