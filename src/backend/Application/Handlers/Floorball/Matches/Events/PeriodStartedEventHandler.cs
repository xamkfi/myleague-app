// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Commands.Floorball.MatchEvent;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches.Events
{
    /// <summary>
    /// Handler for starting a period in an event-sourced floorball match.
    /// </summary>
    internal class PeriodStartedEventHandler : IRequestHandler<StartPeriodEventCommand, Result<FloorballMatchDto>>
    {
        private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
        private readonly ILogger<PeriodStartedEventHandler> _logger;

        public PeriodStartedEventHandler(
            IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
            ILogger<PeriodStartedEventHandler> logger)
        {
            _eventSourcedMatchRepository = eventSourcedMatchRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result<FloorballMatchDto>> Handle(StartPeriodEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting period {PeriodNumber} for match {MatchId}", request.periodNumber, request.matchId);

                // Load aggregate
                EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.matchId, cancellationToken);

                // Start the requested period
                match.StartPeriod(request.periodNumber);

                // Persist events
                await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

                // Map aggregate to DTO (team names still placeholders)
                FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, "Home Team", "Away Team");

                _logger.LogInformation("Successfully started period {PeriodNumber} for match {MatchId}", request.periodNumber, request.matchId);

                return Result<FloorballMatchDto>.Success(matchDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Match not found: {MatchId}", request.matchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.matchId} not found.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while starting period {PeriodNumber} for match: {MatchId}", request.periodNumber, request.matchId);
                return Result<FloorballMatchDto>.Failure(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments while starting period {PeriodNumber} for match: {MatchId}", request.periodNumber, request.matchId);
                return Result<FloorballMatchDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while starting period {PeriodNumber} for match: {MatchId}", request.periodNumber, request.matchId);
                return Result<FloorballMatchDto>.Failure("An error occurred while starting the period.");
            }
        }
    }
}
