using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.Clubs.Handlers;

/// <summary>
/// Handler for updating a club's logo
/// </summary>
public class UpdateClubLogoHandler : IRequestHandler<UpdateClubLogoCommand, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateClubLogoHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateClubLogoHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateClubLogoHandler(IClubRepository clubRepository, IUnitOfWork unitOfWork, ILogger<UpdateClubLogoHandler> logger)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateClubLogoCommand request
    /// </summary>
    /// <param name="request">The command containing the club ID and new logo URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated club as a DTO wrapped in a Result</returns>
    public async Task<Result<ClubDto>> Handle(UpdateClubLogoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing club
            Club? existingClub = await _clubRepository.GetByIdAsync(request.ClubId);
            if (existingClub == null)
            {
                _logger.LogWarning("Attempt to update logo for non-existent club with ID: {ClubId}", request.ClubId);
                return Result<ClubDto>.NotFound("Club", request.ClubId);
            }

            // Update the club's logo using the UpdateOnlinePresence method
            Uri? logoUri = !string.IsNullOrEmpty(request.LogoUrl) ? new Uri(request.LogoUrl) : null;
            existingClub.UpdateOnlinePresence(existingClub.WebsiteUrl, logoUri, existingClub.ContactEmail);
            
            _logger.LogInformation("Updating logo for club: {ClubId} - {ClubName}", existingClub.Id, existingClub.Name);
            await _clubRepository.UpdateAsync(existingClub);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            ClubDto clubDto = ClubMapper.ToDto(existingClub);
            _logger.LogInformation("Successfully updated logo for club with ID: {ClubId}", existingClub.Id);

            return Result<ClubDto>.Success(clubDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating logo for club: {ClubId}", request.ClubId);
            return Result<ClubDto>.Failure("An error occurred while updating the club logo.");
        }
    }
} 
