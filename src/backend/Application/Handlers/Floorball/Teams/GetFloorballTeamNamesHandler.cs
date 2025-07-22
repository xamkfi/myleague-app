// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Team;
using Domain.Repositories.Floorball;
using Domain.Entities.Floorball;
using Application.Common;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Teams
{
    /// <summary>
    /// Handler to retrieve floorball team names with optional filter
    /// </summary>
    public class GetFloorballTeamNamesHandler : IRequestHandler<GetTeamNamesQuery, Result<List<FloorballTeamNameDto>>>
    {
        private readonly IFloorballTeamRepository _floorballTeamRepository;
        private readonly ILogger<GetFloorballTeamNamesHandler> _logger;

        public GetFloorballTeamNamesHandler(
            IFloorballTeamRepository floorballTeamRepository,
            ILogger<GetFloorballTeamNamesHandler> logger
            )
        {
            _floorballTeamRepository = floorballTeamRepository;
            _logger = logger;
        }

        public async Task<Result<List<FloorballTeamNameDto>>> Handle(GetTeamNamesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving team names with filter: {Filter}", request.NameFilter);

                IEnumerable<FloorballTeam> teams = await _floorballTeamRepository.GetByNameFilterAsync(request.NameFilter, cancellationToken);

                List<FloorballTeamNameDto> dtos = teams
                    .Select(team => new FloorballTeamNameDto
                    {
                        Id = team.Id,
                        Name = team.Name,
                    }).ToList();

                return Result<List<FloorballTeamNameDto>>.Success(dtos);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retreiving team names");
                return Result<List<FloorballTeamNameDto>>.Failure("An error occurred while retrieving team names");
            }

        }
    }
}
