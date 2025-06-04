using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Team;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for retrieving a floorball team by ID
/// </summary>
public class GetFloorballTeamByIdHandler : IRequestHandler<GetFloorballTeamByIdQuery, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetFloorballTeamByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTeamByIdHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTeamByIdHandler(
        IFloorballTeamRepository teamRepository,
        ILogger<GetFloorballTeamByIdHandler> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTeamByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The team as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(GetFloorballTeamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball team with ID: {TeamId}", request.Id);
            
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.Id);
            if (team == null)
            {
                _logger.LogWarning("Floorball team with ID {TeamId} not found", request.Id);
                return Result<FloorballTeamDto>.NotFound("FloorballTeam", request.Id);
            }

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team);
            _logger.LogInformation("Successfully retrieved floorball team: {TeamId}", team.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball team: {TeamId}", request.Id);
            return Result<FloorballTeamDto>.Failure("An error occurred while retrieving the floorball team.");
        }
    }
} 
