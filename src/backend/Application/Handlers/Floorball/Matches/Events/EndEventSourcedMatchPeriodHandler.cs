// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Floorball.MatchEvent;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches.Events
{
    public class EndEventSourcedMatchPeriodHandler : IRequestHandler<EndEventSourcedMatchPeriodCommand, Result<FloorballMatchDto>>
    {
        private readonly IEventSourcedFloorballMatchRepository _eventSourcedFloorballMatchRepository;
        private readonly ILogger<DeletePenaltyEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the EndEventSourcedMatchPeriodHandler class
        /// </summary>
        /// <param name="matchRepository">The floorball match repository</param>
        /// <param name="logger">The logger</param>
        public EndEventSourcedMatchPeriodHandler(
            IEventSourcedFloorballMatchRepository eventSourcedFloorballMatchRepository,
            ILogger<DeletePenaltyEventHandler> logger)
        {
            _eventSourcedFloorballMatchRepository = eventSourcedFloorballMatchRepository;
            _logger = logger;
        }

        public async Task<Result<FloorballMatchDto>> Handle(EndEventSourcedMatchPeriodCommand request, CancellationToken cancellationToken)
        {
            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedFloorballMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            match.EndPeriod(request.periodNumber);

            await _eventSourcedFloorballMatchRepository.SaveAsync(match);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);

            return Result<FloorballMatchDto>.Success(matchDto); ;
        }
    }
}
