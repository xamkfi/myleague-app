using System;
using System.Collections.Generic;
using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving standings for a single tournament group.
/// </summary>
/// <param name="GroupId">The tournament group ID</param>
public record GetTournamentGroupStandingsQuery(Guid GroupId)
    : IRequest<Result<List<FloorballTournamentGroupStandingDto>>>;
