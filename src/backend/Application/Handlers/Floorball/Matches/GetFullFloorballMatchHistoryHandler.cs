using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Match;
using Domain.DomainEvents;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches
{
    public class GetFullFloorballMatchHistoryHandler : IRequestHandler<GetFullFloorballMatchHistoryQuery, Result<IEnumerable<FloorballDomainEventDto>>>
    {
        private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
        private readonly ILogger<GetFullFloorballMatchHistoryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetFloorballMatchesBySeasonHandler class
        /// </summary>
        /// <param name="matchRepository">The floorball match repository</param>
        /// <param name="logger">The logger</param>
        public GetFullFloorballMatchHistoryHandler(
            IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
            ILogger<GetFullFloorballMatchHistoryHandler> logger)
        {
            _eventSourcedMatchRepository = eventSourcedMatchRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<FloorballDomainEventDto>>> Handle(GetFullFloorballMatchHistoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving full event history for match: {MatchId}", request.MatchId);

                IEnumerable<IDomainEvent> events = await _eventSourcedMatchRepository.GetHistoryAsync(request.MatchId, cancellationToken);

                IEnumerable<FloorballDomainEventDto> dtos = events
                    .OrderBy(e => e.OccurredOn)
                    .Select(e => new FloorballDomainEventDto(e.GetType().Name, e.OccurredOn, e))
                    .ToList();

                return Result<IEnumerable<FloorballDomainEventDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event history for match: {MatchId}", request.MatchId);
                return Result<IEnumerable<FloorballDomainEventDto>>.Failure("An error occurred while retrieving match history.");
            }
        }
    }
}
