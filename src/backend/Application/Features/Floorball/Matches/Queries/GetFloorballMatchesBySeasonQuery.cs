using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Queries
{
    /// <summary>
    /// Query for retrieving all floorball matches in a season
    /// </summary>
    /// <param name="SeasonId"></param>
    public record GetFloorballMatchesBySeasonQuery(Guid CompetitionId) : IRequest<Result<IEnumerable<FloorballMatchDto>>>;
}
