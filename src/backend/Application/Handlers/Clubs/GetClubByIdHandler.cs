using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Queries.Clubs;
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
public class GetClubByIdHandler : IRequestHandler<GetClubByIdQuery, ClubDto>
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
    /// <returns>The club as DTO if found</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when the club is not found</exception>
    public async Task<ClubDto> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
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
        
        if (club == null)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
            throw new InvalidOperationException($"Club with ID '{request.ClubId}' not found.");
        }

        return ClubMapper.ToDto(club);
    }
} 