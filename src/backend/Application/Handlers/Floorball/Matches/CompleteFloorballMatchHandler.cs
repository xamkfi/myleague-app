using Application.Commands.Floorball.Match;
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

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for completing a floorball match
/// </summary>
public class CompleteFloorballMatchHandler : IRequestHandler<CompleteFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CompleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CompleteFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CompleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            _logger.LogInformation("Completing floorball match: {MatchId}", request.Id);
            match.Complete();
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully completed floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while completing the match.");
        }
    }
} 
