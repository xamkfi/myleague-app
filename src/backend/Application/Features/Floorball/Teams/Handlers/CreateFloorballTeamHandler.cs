using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for creating a new floorball team
/// </summary>
public class CreateFloorballTeamHandler : IRequestHandler<CreateFloorballTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(CreateFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                return Result<FloorballTeamDto>.NotFound("Club", request.ClubId);
            }
            // Create the team entity
            FloorballTeam team = FloorballTeamMapper.ToEntity(request, club);

            _logger.LogInformation("Creating new floorball team: {TeamName}", request.Name);
            await _teamRepository.AddAsync(team);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team, club);
            _logger.LogInformation("Successfully created floorball team with ID: {TeamId}", team.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball team: {TeamName}", request.Name);
            return Result<FloorballTeamDto>.Failure("An error occurred while creating the floorball team.");
        }
    }
} 
