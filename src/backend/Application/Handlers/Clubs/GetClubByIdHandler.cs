using Application.Queries.Clubs;
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
/// Handler for retrieving a club by its ID
/// </summary>
public class GetClubByIdHandler : IRequestHandler<GetClubByIdQuery, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetClubByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetClubByIdHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetClubByIdHandler(IClubRepository clubRepository, ILogger<GetClubByIdHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetClubByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the club ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The club as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<ClubDto>> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving club with ID: {ClubId}", request.ClubId);
            
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
                return Result<ClubDto>.NotFound("Club", request.ClubId);
            }

            ClubDto clubDto = ClubMapper.ToDto(club);
            _logger.LogInformation("Successfully retrieved club: {ClubId} - {ClubName}", club.Id, club.Name);

            return Result<ClubDto>.Success(clubDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving club: {ClubId}", request.ClubId);
            return Result<ClubDto>.Failure("An error occurred while retrieving the club.");
        }
    }
} 