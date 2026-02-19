// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;
using Application.DTOs.Floorball;

namespace Application.Queries.Floorball.Team
{
    /// <summary>
    /// Query for retrieving the names of floorball teams with an optional filter
    /// </summary>
    /// <param name="NameFilter"></param>
    public record GetFloorballTeamNamesQuery(string? NameFilter = null) : IRequest<Result<List<FloorballTeamNameDto>>>;
}
