using Application.Features.Football.Teams.Commands;
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
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for updating an existing football team
/// </summary>
public class UpdateFootballTeamHandler : IRequestHandler<UpdateFootballTeamCommand, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFootballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFootballTeamHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFootballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing updated team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamDto>> Handle(UpdateFootballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            
            // Find the existing team
            FootballTeam? existingTeam = await _teamRepository.GetByIdAsync(request.Id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Attempt to update non-existent football team with ID: {TeamId}", request.Id);
                return Result<FootballTeamDto>.NotFound("FootballTeam", request.Id);
            }

            // Update the team
            FootballTeamMapper.UpdateFromCommand(existingTeam, request);
            
            _logger.LogInformation("Updating football team: {TeamId}", existingTeam.Id);
            await _teamRepository.UpdateAsync(existingTeam);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(existingTeam.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", existingTeam.ClubId, existingTeam.Id);
                return Result<FootballTeamDto>.Failure("Associated club not found");
            }

            FootballTeamDto teamDto = FootballTeamMapper.ToDto(existingTeam, club);
            _logger.LogInformation("Successfully updated football team with ID: {TeamId}", existingTeam.Id);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football team: {TeamId}", request.Id);
            return Result<FootballTeamDto>.Failure("An error occurred while updating the football team.");
        }
    }
} 
