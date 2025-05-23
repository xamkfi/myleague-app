using Application.Commands.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for creating a new club
/// </summary>
public class CreateClubHandler : IRequestHandler<CreateClubCommand, ClubDto>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<CreateClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateClubHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public CreateClubHandler(IClubRepository clubRepository, ILogger<CreateClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateClubCommand request
    /// </summary>
    /// <param name="request">The command containing club information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created club as a DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public async Task<ClubDto> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (await _clubRepository.ExistsByNameAsync(request.Name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", request.Name);
            throw new InvalidOperationException($"A club with the name '{request.Name}' already exists.");
        }

        Club club = ClubMapper.ToEntity(request);

        _logger.LogInformation("Creating new club: {ClubName}", club.Name);
        await _clubRepository.AddAsync(club);

        return ClubMapper.ToDto(club);
    }
} 