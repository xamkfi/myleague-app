using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class RecordSubstitutionHandler : IRequestHandler<RecordSubstitutionCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordSubstitutionHandler> _logger;

    public RecordSubstitutionHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<RecordSubstitutionHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(RecordSubstitutionCommand request, CancellationToken cancellationToken)
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

            FootballPlayer? playerOff = await _playerRepository.GetByIdAsync(request.PlayerOffId);
            if (playerOff == null)
            {
                return Result<FootballMatchDto>.Failure($"Player going off with ID {request.PlayerOffId} not found.");
            }

            FootballPlayer? playerOn = await _playerRepository.GetByIdAsync(request.PlayerOnId);
            if (playerOn == null)
            {
                return Result<FootballMatchDto>.Failure($"Player coming on with ID {request.PlayerOnId} not found.");
            }

            FootballSubstitution substitution = match.RecordSubstitution(
                team,
                playerOff,
                playerOn,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            _matchRepository.MarkEventAsAdded(substitution);
            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.SubstitutionRecorded,
                new MatchNotificationPayload(match.Id));

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording substitution in match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while recording the substitution.");
        }
    }
}
