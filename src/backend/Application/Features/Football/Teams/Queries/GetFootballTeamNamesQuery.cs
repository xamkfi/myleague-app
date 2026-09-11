// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;

namespace Application.Features.Football.Teams.Queries
{
    /// <summary>
    /// Query for retrieving the names of football teams with an optional filter
    /// </summary>
    /// <param name="NameFilter"></param>
    public record GetFootballTeamNamesQuery(string? NameFilter = null) : IRequest<Result<List<FootballTeamNameDto>>>;
}
