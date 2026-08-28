using Application.Features.Football.Teams.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for retrieving football teams by division
/// </summary>
public class GetFootballTeamsByDivisionHandler : IRequestHandler<GetFootballTeamsByDivisionQuery, Result<IEnumerable<FootballTeamDto>>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFootballTeamsByDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFootballTeamsByDivisionHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFootballTeamsByDivisionHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballTeamsByDivisionHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFootballTeamsByDivisionQuery request
    /// </summary>
    /// <param name="request">The query containing division</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Football teams by division as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FootballTeamDto>>> Handle(GetFootballTeamsByDivisionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football teams for division: {Division}", request.DivisionId);
            
            // Get teams for the division
            IEnumerable<FootballTeam> teams = await _teamRepository.GetByDivisionAsync(request.DivisionId);
            
            // Load all clubs for the teams
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            Dictionary<Guid, Club> clubDictionary = clubs.ToDictionary(c => c.Id);
            
            // Map teams to DTOs with their corresponding clubs
            IEnumerable<FootballTeamDto> teamDtos = FootballTeamMapper.ToDtos(teams, clubDictionary);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} football teams for division: {Division}", teamDtos.Count(), request.DivisionId);
            
            return Result<IEnumerable<FootballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football teams for division: {Division}", request.DivisionId);
            return Result<IEnumerable<FootballTeamDto>>.Failure("An error occurred while retrieving football teams.");
        }
    }
} 
