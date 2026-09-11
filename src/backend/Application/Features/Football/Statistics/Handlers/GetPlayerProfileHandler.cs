using Application.Common;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Gets a player full profile with all statistics from seasons.
/// </summary>
public class GetPlayerProfileHandler : IRequestHandler<GetPlayerProfileQuery, Result<FootballPlayerProfileDto>>
{
    private readonly IFootballStatisticsRepository _footballStatisticsRepository;
    private readonly IFootballPlayerRepository _footballPlayerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetPlayerProfileHandler> _logger;

    public GetPlayerProfileHandler(
        IFootballStatisticsRepository footballStatisticsRepository,
        IFootballPlayerRepository footballPlayerRepository,
        IPersonRepository personRepository,
        ILogger<GetPlayerProfileHandler> logger)
    {
        _footballStatisticsRepository = footballStatisticsRepository;
        _footballPlayerRepository = footballPlayerRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<FootballPlayerProfileDto>> Handle(GetPlayerProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            FootballPlayer? player = await _footballPlayerRepository.GetByIdAsync(request.playerId);

            if (player == null)
            {
                _logger.LogWarning("Player not found by id {id}", request.playerId);
                return Result<FootballPlayerProfileDto>.NotFound("Player not found by id {id}.", request.playerId);
            }

            Person? person = await _personRepository.GetByIdAsync(player.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Person not found by player id {id}", request.playerId);
                return Result<FootballPlayerProfileDto>.NotFound("Person not found by player id {id}.", request.playerId);
            }

            List<FootballPlayerSeasonStatistics> playerSeasonStatistics =
                await _footballStatisticsRepository.GetPlayerCareerStatisticsAsync(request.playerId);

            FootballPlayerProfileDto playerProfile = FootballStatisticsMapper.ToDto(player, person, playerSeasonStatistics);

            _logger.LogInformation("Succesfully retrieved player profile for player: {id}", request.playerId);
            return Result<FootballPlayerProfileDto>.Success(playerProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting player profile for Player: {PlayerId}", request.playerId);
            return Result<FootballPlayerProfileDto>.Failure("An error occurred while retrieving player season statistics.");
        }
    }
}
