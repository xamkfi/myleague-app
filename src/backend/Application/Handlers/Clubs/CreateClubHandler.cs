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
/// Handler for creating a new club
/// </summary>
public class CreateClubHandler : IRequestHandler<CreateClubCommand, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateClubHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateClubHandler(IClubRepository clubRepository, IUnitOfWork unitOfWork, ILogger<CreateClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateClubCommand request
    /// </summary>
    /// <param name="request">The command containing club information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created club as a DTO wrapped in a Result</returns>
    public async Task<Result<ClubDto>> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if a club with the same name already exists
            if (await _clubRepository.ExistsByNameAsync(request.Name))
            {
                _logger.LogWarning("Attempt to create club with existing name: {ClubName}", request.Name);
                return Result<ClubDto>.Failure($"A club with the name '{request.Name}' already exists.");
            }

            // Create the club entity
            Club club = ClubMapper.ToEntity(request);

            _logger.LogInformation("Creating new club: {ClubName}", club.Name);
            await _clubRepository.AddAsync(club);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            ClubDto clubDto = ClubMapper.ToDto(club);
            _logger.LogInformation("Successfully created club with ID: {ClubId}", club.Id);

            return Result<ClubDto>.Success(clubDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating club: {ClubName}", request.Name);
            return Result<ClubDto>.Failure("An error occurred while creating the club.");
        }
    }
} 