
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics
{
    /// <summary>
    /// Gets a player full profile with all statistics from seasons.
    /// </summary>
    public class GetPlayerProfileHandler : IRequestHandler<GetPlayerProfileQuery, Result<FloorballPlayerProfileDto>>
    {
        /// <summary>
        /// Initializes the instances of repositories
        /// </summary>
        private readonly IFloorballStatisticsRepository _floorballStatisticsRepository;
        private readonly IFloorballPlayerRepository _floorballPlayerRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<GetPlayerProfileHandler> _logger;

        public GetPlayerProfileHandler(IFloorballStatisticsRepository floorballStatisticsRepository,
            IFloorballPlayerRepository floorballPlayerRepository,
            IPersonRepository personRepository,
            ILogger<GetPlayerProfileHandler> logger)
        {
            _floorballStatisticsRepository = floorballStatisticsRepository;
            _floorballPlayerRepository = floorballPlayerRepository;
            _personRepository = personRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetPlayerProfileQuery
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<FloorballPlayerProfileDto>> Handle(GetPlayerProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                FloorballPlayer? player = await _floorballPlayerRepository.GetByIdAsync(request.playerId);

                if (player == null)
                {
                    _logger.LogWarning("Player not found by id {id}", request.playerId);
                    return Result<FloorballPlayerProfileDto>.NotFound("Player not found by id {id}.", request.playerId);
                }

                //Retrieve goalie statistics if player has any.
                 List<FloorballGoalieSeasonStatistics>? goalieCareerStatistics = await _floorballStatisticsRepository.GetGoalieCareerStatisticsAsync(request.playerId);

                Person? person = await _personRepository.GetByIdAsync(player.PersonId);

                //Retrieve player statistics if player has any.
                List<FloorballPlayerSeasonStatistics>? PlayerSeasonStatistics = await _floorballStatisticsRepository.GetPlayerCareerStatisticsAsync(request.playerId);

                FloorballPlayerProfileDto playerProfile = FloorballStatisticsMapper.ToDto(player, person, PlayerSeasonStatistics, goalieCareerStatistics);

                _logger.LogInformation("Succesfully retrieved player profile for player: {id}", request.playerId);
                return Result<FloorballPlayerProfileDto>.Success(playerProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting player profile for Player: {PlayerId}", request.playerId);
                return Result<FloorballPlayerProfileDto>.Failure("An error occurred while retrieving player season statistics.");
            }
        }
    }
}
