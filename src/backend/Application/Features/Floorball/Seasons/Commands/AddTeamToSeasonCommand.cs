using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command for adding a team to a floorball season
    /// </summary>
    /// <param name="SeasonId">The ID of the season to add the team to</param>
    /// <param name="TeamId">The ID of the team to add to the season</param>
    public record AddTeamToSeasonCommand(
        Guid SeasonId,
        Guid TeamId) : IRequest<Result<FloorballSeasonDto>>;
} 