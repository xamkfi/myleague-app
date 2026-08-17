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
/// Handler for updating a football team's logo
/// </summary>
public class UpdateFootballTeamLogoHandler : IRequestHandler<UpdateFootballTeamLogoCommand, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballTeamLogoHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFootballTeamLogoHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFootballTeamLogoHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballTeamLogoHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFootballTeamLogoCommand request
    /// </summary>
    /// <param name="request">The command containing the team ID and new logo URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamDto>> Handle(UpdateFootballTeamLogoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing team
            FootballTeam? existingTeam = await _teamRepository.GetByIdAsync(request.Id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Attempt to update logo for non-existent football team with ID: {TeamId}", request.Id);
                return Result<FootballTeamDto>.NotFound("FootballTeam", request.Id);
            }

            // Update the team's logo
            Uri? logoUri = !string.IsNullOrEmpty(request.LogoUrl) ? new Uri(request.LogoUrl) : null;
            existingTeam.UpdateLogo(logoUri);
            
            _logger.LogInformation("Updating logo for football team: {TeamId}", existingTeam.Id);
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
            _logger.LogInformation("Successfully updated logo for football team with ID: {TeamId}", existingTeam.Id);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating logo for football team: {TeamId}", request.Id);
            return Result<FootballTeamDto>.Failure("An error occurred while updating the team logo.");
        }
    }
} 
