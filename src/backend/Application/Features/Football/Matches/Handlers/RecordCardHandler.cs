using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class RecordCardHandler : IRequestHandler<RecordCardCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordCardHandler> _logger;

    public RecordCardHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<RecordCardHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(RecordCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FootballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FootballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                return Result<FootballMatchDto>.Failure($"Player with ID {request.PlayerId} not found.");
            }

            FootballCard card = match.RecordCard(
                team,
                player,
                request.CardType,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            await UpdatePlayerCardStatistics(card, match.CompetitionId, increment: true, cancellationToken);

            _matchRepository.MarkEventAsAdded(card);
            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.CardAssigned,
                new MatchNotificationPayload(match.Id));

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording card in match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while recording the card.");
        }
    }

    internal static async Task UpdatePlayerCardStatistics(
        FootballCard card,
        Guid competitionId,
        bool increment,
        IFootballStatisticsRepository statisticsRepository,
        CancellationToken cancellationToken)
    {
        FootballPlayerSeasonStatistics? playerStats =
            await statisticsRepository.GetPlayerSeasonStatisticsAsync(
                card.PlayerId,
                card.TeamId,
                competitionId,
                cancellationToken);
        playerStats ??= new FootballPlayerSeasonStatistics(card.PlayerId, card.TeamId, competitionId);

        if (card.CardType == FootballCardType.Yellow)
        {
            if (increment)
            {
                playerStats.RecordYellowCard();
            }
            else
            {
                playerStats.RemoveYellowCard();
            }
        }
        else
        {
            if (increment)
            {
                playerStats.RecordRedCard();
            }
            else
            {
                playerStats.RemoveRedCard();
            }
        }

        await statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }

    private Task UpdatePlayerCardStatistics(
        FootballCard card,
        Guid competitionId,
        bool increment,
        CancellationToken cancellationToken) =>
        UpdatePlayerCardStatistics(card, competitionId, increment, _statisticsRepository, cancellationToken);
}
