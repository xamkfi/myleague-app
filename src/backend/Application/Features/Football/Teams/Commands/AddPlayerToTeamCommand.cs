using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for adding a new player to a football team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="Position"></param>
    /// <param name="JerseyNumber"></param>
    public record AddPlayerToTeamCommand(
        Guid TeamId,
        Guid PlayerId,
        FootballPosition Position,
        int? JerseyNumber,
        /// <summary>
        /// The jersey number the caller originally wanted. When it differs from
        /// <paramref name="JerseyNumber"/>, the resulting roster entry is flagged so the
        /// roster UI can highlight it for admin review. <c>null</c> (or equal to
        /// <paramref name="JerseyNumber"/>) means no substitution occurred.
        /// </summary>
        int? RequestedJerseyNumber = null) : IRequest<Result<FootballTeamDto>>;
}
