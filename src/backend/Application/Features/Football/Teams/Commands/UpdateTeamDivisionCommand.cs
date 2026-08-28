// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for updating a division from team
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="divisionId"></param>
    public record UpdateTeamDivisionCommand(Guid teamId, Guid divisionId) : IRequest<Result<FootballTeamDto>>;
}
