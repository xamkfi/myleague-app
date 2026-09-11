// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Queries;
using Domain.Repositories.Football;
using Domain.Entities.Football.Teams;
using Application.Common;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Teams.Handlers
{
    /// <summary>
    /// Handler to retrieve football team names with optional filter
    /// </summary>
    public class GetFootballTeamNamesHandler : IRequestHandler<GetTeamNamesQuery, Result<List<FootballTeamNameDto>>>
    {
        private readonly IFootballTeamRepository _footballTeamRepository;
        private readonly ILogger<GetFootballTeamNamesHandler> _logger;

        public GetFootballTeamNamesHandler(
            IFootballTeamRepository footballTeamRepository,
            ILogger<GetFootballTeamNamesHandler> logger
            )
        {
            _footballTeamRepository = footballTeamRepository;
            _logger = logger;
        }

        public async Task<Result<List<FootballTeamNameDto>>> Handle(GetTeamNamesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving team names with filter: {Filter}", request.NameFilter);

                IEnumerable<FootballTeam> teams = await _footballTeamRepository.GetByNameFilterAsync(request.NameFilter, cancellationToken);

                List<FootballTeamNameDto> dtos = teams
                    .Select(team => new FootballTeamNameDto
                    {
                        Id = team.Id,
                        Name = team.Name,
                    }).ToList();

                return Result<List<FootballTeamNameDto>>.Success(dtos);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retreiving team names");
                return Result<List<FootballTeamNameDto>>.Failure("An error occurred while retrieving team names");
            }

        }
    }
}
