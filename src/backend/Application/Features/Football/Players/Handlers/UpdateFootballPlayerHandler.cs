using Application.Features.Football.Players.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for updating an existing football player
/// </summary>
public class UpdateFootballPlayerHandler : IRequestHandler<UpdateFootballPlayerCommand, Result<FootballPlayerDto>>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFootballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFootballPlayerHandler(
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork, 
        ILogger<UpdateFootballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFootballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing updated player information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated player as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballPlayerDto>> Handle(UpdateFootballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing player
            FootballPlayer? existingPlayer = await _playerRepository.GetByIdAsync(request.Id);
            if (existingPlayer == null)
            {
                _logger.LogWarning("Attempt to update non-existent football player with ID: {PlayerId}", request.Id);
                return Result<FootballPlayerDto>.NotFound("FootballPlayer", request.Id);
            }

            // Update the player
            FootballPlayerMapper.UpdateFromCommand(existingPlayer, request);
            
            _logger.LogInformation("Updating football player: {PlayerId}", existingPlayer.Id);
            
            // Save changes explicitly to trigger domain events
            // Note: No need to call UpdateAsync since the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load Person data to create proper DTO response
            Person? person = await _personRepository.GetByIdAsync(existingPlayer.PersonId);
            FootballPlayerDto playerDto;
            
            if (person != null)
            {
                // Create DTO with real person data
                playerDto = new FootballPlayerDto(
                    existingPlayer.Id,
                    existingPlayer.PersonId,
                    PersonMapper.ToDto(person),
                    existingPlayer.IsActive,
                    existingPlayer.Position.PrimaryPosition,
                    existingPlayer.CareerGoals,
                    existingPlayer.CareerAssists,
                    null // Team information not retrieved in update handler
                );
            }
            else
            {
                // Fallback to placeholder if person not found
                playerDto = FootballPlayerMapper.ToDto(existingPlayer);
            }
            
            _logger.LogInformation("Successfully updated football player with ID: {PlayerId}", existingPlayer.Id);

            return Result<FootballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football player: {PlayerId}", request.Id);
            return Result<FootballPlayerDto>.Failure("An error occurred while updating the football player.");
        }
    }
} 
