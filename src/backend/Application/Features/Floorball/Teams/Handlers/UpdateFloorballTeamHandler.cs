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
/// Handler for updating an existing floorball team
/// </summary>
public class UpdateFloorballTeamHandler : IRequestHandler<UpdateFloorballTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing updated team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(UpdateFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            
            // Find the existing team
            FloorballTeam? existingTeam = await _teamRepository.GetByIdAsync(request.Id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball team with ID: {TeamId}", request.Id);
                return Result<FloorballTeamDto>.NotFound("FloorballTeam", request.Id);
            }

            // Update the team
            FloorballTeamMapper.UpdateFromCommand(existingTeam, request);
            
            _logger.LogInformation("Updating floorball team: {TeamId}", existingTeam.Id);
            await _teamRepository.UpdateAsync(existingTeam);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(existingTeam.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", existingTeam.ClubId, existingTeam.Id);
                return Result<FloorballTeamDto>.Failure("Associated club not found");
            }

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(existingTeam, club);
            _logger.LogInformation("Successfully updated floorball team with ID: {TeamId}", existingTeam.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball team: {TeamId}", request.Id);
            return Result<FloorballTeamDto>.Failure("An error occurred while updating the floorball team.");
        }
    }
} 
