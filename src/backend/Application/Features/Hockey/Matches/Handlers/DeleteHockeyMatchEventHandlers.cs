using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Deletes a goal event and reverses the match scoreboard side effect.
/// </summary>
public class DeleteHockeyGoalHandler : IRequestHandler<DeleteHockeyGoalCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteHockeyGoalHandler> _logger;

    public DeleteHockeyGoalHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeleteHockeyGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        DeleteHockeyGoalCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            HockeyGoal deleted = match.DeleteGoalEvent(request.GoalEventId);
            _matchRepository.MarkEventAsDeleted(deleted);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Deleted goal {GoalEventId} from match {MatchId}",
                request.GoalEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected DeleteHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid DeleteHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed DeleteHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while deleting the goal.", ex.Flatten());
        }
    }
}

/// <summary>
/// Deletes a penalty event from a hockey match.
/// </summary>
public class DeleteHockeyPenaltyHandler : IRequestHandler<DeleteHockeyPenaltyCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteHockeyPenaltyHandler> _logger;

    public DeleteHockeyPenaltyHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeleteHockeyPenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        DeleteHockeyPenaltyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            HockeyPenalty deleted = match.DeletePenaltyEvent(request.PenaltyEventId);
            _matchRepository.MarkEventAsDeleted(deleted);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Deleted penalty {PenaltyEventId} from match {MatchId}",
                request.PenaltyEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected DeleteHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid DeleteHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed DeleteHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while deleting the penalty.", ex.Flatten());
        }
    }
}

/// <summary>
/// Deletes a shot event from a hockey match.
/// </summary>
public class DeleteHockeyShotHandler : IRequestHandler<DeleteHockeyShotCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteHockeyShotHandler> _logger;

    public DeleteHockeyShotHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeleteHockeyShotHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        DeleteHockeyShotCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            HockeyShot deleted = match.DeleteShotEvent(request.ShotEventId);
            _matchRepository.MarkEventAsDeleted(deleted);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Deleted shot {ShotEventId} from match {MatchId}",
                request.ShotEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected DeleteHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid DeleteHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed DeleteHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while deleting the shot.", ex.Flatten());
        }
    }
}
