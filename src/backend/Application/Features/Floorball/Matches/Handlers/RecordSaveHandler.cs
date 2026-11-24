using Application.Features.Floorball.Matches.Commands;
using Application.Common;
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
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Application.Constants;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for recording a save in a floorball match
/// </summary>
public class RecordSaveHandler : IRequestHandler<RecordFloorballSaveCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordSaveHandler> _logger;

    public RecordSaveHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<RecordSaveHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordFloorballSaveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FloorballPlayer? goalie = await _playerRepository.GetByIdAsync(request.GoalieId);
            if (goalie == null)
            {
                _logger.LogWarning("Goalie not found with ID: {GoalieId}", request.GoalieId);
                return Result<FloorballMatchDto>.Failure($"Goalie with ID {request.GoalieId} not found.");
            }

            // Defensive clamp: the controller validates [1, 99] but legacy callers that
            // construct the command directly (older tests, future internal flows) might pass
            // Count = 0. Treating < 1 as 1 keeps the single-save semantics identical to
            // before this field existed.
            int saveCount = request.Count < 1 ? 1 : request.Count;

            _logger.LogInformation(
                "Recording {Count} save(s) in match {MatchId}", saveCount, request.MatchId);

            // Verify active goalie once (the discrepancy is the same for every save in the
            // bulk batch, so logging it N times would just be noise).
            Guid? activeGoalieId = match.GetActiveGoalieId(request.TeamId);
            if (activeGoalieId.HasValue && activeGoalieId.Value != goalie.Id)
            {
                _logger.LogWarning("Save recorded for goalie {GoalieId} but active goalie is {ActiveGoalieId} for team {TeamId}",
                    goalie.Id, activeGoalieId.Value, request.TeamId);
                // Still record the save(s) but log the discrepancy.
            }

            for (int i = 0; i < saveCount; i++)
            {
                FloorballSave saveEvent = match.RecordSave(
                    team,
                    goalie,
                    request.PeriodNumber,
                    request.TimeInSeconds,
                    request.WasInOvertime,
                    request.WasInShootout);

                _matchRepository.MarkEventAsAdded(saveEvent);
            }

            // Aggregate stats updates so we hit the statistics tables once per request rather
            // than once per save.
            await FloorballMatchStatisticsSupport.AdjustGoalieAsync(
                _statisticsRepository, goalie.Id, request.TeamId, match.CompetitionId, saveCount, goalsAgainst: 0, cancellationToken);
            await FloorballMatchStatisticsSupport.SyncMatchShotsAsync(_statisticsRepository, match, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Single notification regardless of bulk size — listeners (e.g. spectator pages,
            // analytics) only need one cue to refresh and we don't want to spam them with N
            // identical events.
            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.SaveRecorded,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordSave for {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordSave for {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }
}
