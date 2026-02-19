using Application.Commands.Floorball.Team;
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
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for updating a floorball team's logo
/// </summary>
public class UpdateFloorballTeamLogoHandler : IRequestHandler<UpdateFloorballTeamLogoCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballTeamLogoHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballTeamLogoHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballTeamLogoHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballTeamLogoHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballTeamLogoCommand request
    /// </summary>
    /// <param name="request">The command containing the team ID and new logo URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(UpdateFloorballTeamLogoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing team
            FloorballTeam? existingTeam = await _teamRepository.GetByIdAsync(request.Id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Attempt to update logo for non-existent floorball team with ID: {TeamId}", request.Id);
                return Result<FloorballTeamDto>.NotFound("FloorballTeam", request.Id);
            }

            // Update the team's logo
            Uri? logoUri = !string.IsNullOrEmpty(request.LogoUrl) ? new Uri(request.LogoUrl) : null;
            existingTeam.UpdateLogo(logoUri);
            
            _logger.LogInformation("Updating logo for floorball team: {TeamId}", existingTeam.Id);
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
            _logger.LogInformation("Successfully updated logo for floorball team with ID: {TeamId}", existingTeam.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating logo for floorball team: {TeamId}", request.Id);
            return Result<FloorballTeamDto>.Failure("An error occurred while updating the team logo.");
        }
    }
} 