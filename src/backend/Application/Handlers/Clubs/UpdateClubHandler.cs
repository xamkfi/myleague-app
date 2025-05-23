using Application.Commands.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for updating an existing club
/// </summary>
public class UpdateClubHandler : IRequestHandler<UpdateClubCommand, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<UpdateClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateClubHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public UpdateClubHandler(IClubRepository clubRepository, ILogger<UpdateClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateClubCommand request
    /// </summary>
    /// <param name="request">The command containing updated club information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated club as a DTO wrapped in a Result</returns>
    public async Task<Result<ClubDto>> Handle(UpdateClubCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing club
            Club? existingClub = await _clubRepository.GetByIdAsync(request.ClubId);
            if (existingClub == null)
            {
                _logger.LogWarning("Attempt to update non-existent club with ID: {ClubId}", request.ClubId);
                return Result<ClubDto>.NotFound("Club", request.ClubId);
            }

            // Check if another club with the same name exists (excluding current club)
            Club? clubWithSameName = await _clubRepository.GetByNameAsync(request.Name);
            if (clubWithSameName != null && clubWithSameName.Id != request.ClubId)
            {
                _logger.LogWarning("Attempt to update club {ClubId} with existing name: {ClubName}", request.ClubId, request.Name);
                return Result<ClubDto>.Failure($"A club with the name '{request.Name}' already exists.");
            }

            // Update the club
            ClubMapper.UpdateFromCommand(existingClub, request);
            
            _logger.LogInformation("Updating club: {ClubId} - {ClubName}", existingClub.Id, existingClub.Name);
            await _clubRepository.UpdateAsync(existingClub);

            ClubDto clubDto = ClubMapper.ToDto(existingClub);
            _logger.LogInformation("Successfully updated club with ID: {ClubId}", existingClub.Id);

            return Result<ClubDto>.Success(clubDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating club: {ClubId}", request.ClubId);
            return Result<ClubDto>.Failure("An error occurred while updating the club.");
        }
    }
} 