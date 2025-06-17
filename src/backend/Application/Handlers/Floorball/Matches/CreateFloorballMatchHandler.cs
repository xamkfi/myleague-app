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
using Application.Commands.Floorball.Match;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for creating a new floorball match
/// </summary>
public class CreateFloorballMatchHandler : IRequestHandler<CreateFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonRepository seasonRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CreateFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Fetch season object
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.SeasonId);
            if (season==null)
            {
                _logger.LogWarning("Attempt to create match for non-existent season with ID: {SeasonId}", request.SeasonId);
                return Result<FloorballMatchDto>.NotFound("FloorballSeason", request.SeasonId);
            }

            // Fetch team objects
            FloorballTeam? homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId);
            FloorballTeam? awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId);
            if (homeTeam==null)
            {
                _logger.LogWarning("Attempt to create match with non-existent home team ID: {TeamId}", request.HomeTeamId);
                return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.HomeTeamId);
            }
            if (awayTeam==null)
            {
                _logger.LogWarning("Attempt to create match with non-existent away team ID: {TeamId}", request.AwayTeamId);
                return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.AwayTeamId);
            }

            // Create the match entity
            FloorballMatch match = FloorballMatchMapper.ToEntity(request, season, homeTeam, awayTeam);

            _logger.LogInformation("Creating new floorball match between teams: {HomeTeamId} vs {AwayTeamId}", 
                request.HomeTeamId, request.AwayTeamId);
            await _matchRepository.AddAsync(match);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully created floorball match with ID: {MatchId}", match.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball match between teams: {HomeTeamId} vs {AwayTeamId}", 
                request.HomeTeamId, request.AwayTeamId);
            return Result<FloorballMatchDto>.Failure("An error occurred while creating the floorball match.");
        }
    }
} 
