using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for retrieving a club by its ID
/// </summary>
public class GetClubByIdUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetClubByIdUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the GetClubByIdUseCase class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetClubByIdUseCase(IClubRepository clubRepository, ILogger<GetClubByIdUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to retrieve a club by its ID
    /// </summary>
    /// <param name="request">The request containing the club ID</param>
    /// <returns>The club as DTO if found, null otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    public async Task<ClubDto?> ExecuteAsync(GetClubByIdRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.ClubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(request.ClubId));
        }

        _logger.LogInformation("Retrieving club with ID: {ClubId}", request.ClubId);
        Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
        return club != null ? ClubMapper.ToDto(club) : null;
    }
} 
