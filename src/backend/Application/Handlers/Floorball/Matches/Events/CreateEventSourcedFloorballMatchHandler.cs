using Application.Commands.Floorball.MatchEvent;
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
using System.Collections.Generic;
using Application.Commands.Floorball.Match;

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for creating an event-sourced floorball match
/// </summary>
public class CreateEventSourcedFloorballMatchHandler : IRequestHandler<CreateEventSourcedFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<CreateEventSourcedFloorballMatchHandler> _logger;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the CreateEventSourcedFloorballMatchHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public CreateEventSourcedFloorballMatchHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<CreateEventSourcedFloorballMatchHandler> logger,
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonRepository seasonRepository,
        IFloorballUnitOfWork unitOfWork)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the CreateEventSourcedFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CreateEventSourcedFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating event-sourced floorball match: {MatchId}", request.Id);

            // Create the event sourced match using the static factory method
            EventSourcedFloorballMatch match = EventSourcedFloorballMatch.Create(
                request.Id,
                request.SeasonId,
                request.HomeTeamId,
                request.AwayTeamId,
                request.ScheduledDateTime,
                request.Venue);

            // Save the match with its creation event
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

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

            CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(request.SeasonId, request.HomeTeamId, request.AwayTeamId, request.ScheduledDateTime, request.Venue);
            // Create the match entity
            FloorballMatch match2 = FloorballMatchMapper.ToEntity(command, season, homeTeam, awayTeam);

            await _matchRepository.AddAsync(match2);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create the DTO response (simplified for event sourcing)
            var matchDto = new FloorballMatchDto(
                match.Id,
                match.SeasonId,
                match.HomeTeamId,
                homeTeam.Name,
                match.AwayTeamId,
                awayTeam.Name,
                match.ScheduledDateTime,
                match.Venue,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                match.WentToOvertime,
                match.WentToShootout,
                match.PeriodScores,
                match.OfficialIds,
                new List<FloorballGoalEventDto>(), // Empty for now
                new List<FloorballPenaltyEventDto>() // Empty for now
            );

            _logger.LogInformation("Successfully created event-sourced floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments while creating event-sourced match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating event-sourced floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while creating the match.");
        }
    }
} 
