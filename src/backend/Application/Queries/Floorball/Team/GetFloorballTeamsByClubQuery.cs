using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Team
{
    /// <summary>
    /// Query for retrieving all floorball teams in a club
    /// </summary>
    /// <param name="ClubId"></param>
    public record GetFloorballTeamsByClubQuery(Guid ClubId) : IRequest<Result<IEnumerable<FloorballTeamDto>>>;
}
