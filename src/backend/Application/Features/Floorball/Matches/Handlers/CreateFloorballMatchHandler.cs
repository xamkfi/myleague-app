using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Floorball.Matches.Commands;
using Domain.Repositories.Common;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for creating a new floorball match
/// </summary>
public class CreateFloorballMatchHandler : IRequestHandler<CreateFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="unitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballCompetitionRepository seasonRepository,
        IFloorballRefereeRepository refereeRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _refereeRepository = refereeRepository;
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
            if (!request.CompetitionId.HasValue)
                return Result<FloorballMatchDto>.Failure("Competition is required");

            // Fetch competition object
            FloorballCompetition? competition = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (competition == null)
            {
                _logger.LogWarning("Attempt to create match for non-existent competition with ID: {CompetitionId}", request.CompetitionId);
                return Result<FloorballMatchDto>.NotFound("FloorballCompetition", request.CompetitionId ?? Guid.Empty);
            }

            // Teams are optional at creation: a future fixture can be scheduled before its
            // participants are known. When a team ID is provided, however, it must resolve to an
            // existing team — otherwise the admin has typoed an ID and we surface a NotFound.
            FloorballTeam? homeTeam = null;
            if (request.HomeTeamId.HasValue)
            {
                homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId.Value);
                if (homeTeam == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent home team ID: {TeamId}", request.HomeTeamId);
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.HomeTeamId.Value);
                }
            }

            FloorballTeam? awayTeam = null;
            if (request.AwayTeamId.HasValue)
            {
                awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId.Value);
                if (awayTeam == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent away team ID: {TeamId}", request.AwayTeamId);
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.AwayTeamId.Value);
                }
            }

            // Fetch referee if provided
            FloorballReferee? referee = null;
            if (request.RefereeId.HasValue)
            {
                referee = await _refereeRepository.GetByIdAsync(request.RefereeId.Value);
                if (referee == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent referee ID: {RefereeId}", request.RefereeId);
                    return Result<FloorballMatchDto>.NotFound("FloorballReferee", request.RefereeId.Value);
                }
            }

            // Create the match entity
            FloorballMatch match = FloorballMatchMapper.ToEntity(request, competition, homeTeam, awayTeam, referee);

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
