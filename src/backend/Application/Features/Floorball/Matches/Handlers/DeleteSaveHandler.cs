using Application.Features.Floorball.Matches.Commands;
using Application.Common;
using Application.Constants;
using Application.Interfaces.Common;
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
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for deleting a save event from a match
/// </summary>
public class DeleteSaveHandler : IRequestHandler<DeleteFloorballSaveCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<DeleteSaveHandler> _logger;

    public DeleteSaveHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<DeleteSaveHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeleteFloorballSaveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting save {SaveId} from match {MatchId}", request.SaveEventId, request.MatchId);

            // Delete the save event and get the deleted save
            FloorballSave deletedSave = match.DeleteSaveEvent(request.SaveEventId);

            await FloorballMatchStatisticsSupport.AdjustGoalieAsync(
                _statisticsRepository, deletedSave.GoalieId, deletedSave.TeamId, match.CompetitionId, saves: -1, goalsAgainst: 0, cancellationToken);
            await FloorballMatchStatisticsSupport.SyncMatchShotsAsync(_statisticsRepository, match, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.SaveRecorded,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected deleting save {SaveEventId}", request.SaveEventId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Save {SaveEventId} cannot be deleted", request.SaveEventId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }
}
