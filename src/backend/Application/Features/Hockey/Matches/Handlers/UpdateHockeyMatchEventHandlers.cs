using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Corrects a goal event and adjusts the scoreboard when the scoring team changes.
/// </summary>
public class UpdateHockeyGoalHandler : IRequestHandler<UpdateHockeyGoalCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyGoalHandler> _logger;

    public UpdateHockeyGoalHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        UpdateHockeyGoalCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            match.UpdateGoalEvent(
                request.GoalEventId,
                request.ScoringMatchTeamId,
                request.ScorerActivePlayerId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.GoalStrength,
                request.PrimaryAssistActivePlayerId,
                request.SecondaryAssistActivePlayerId,
                request.GoalieActivePlayerId,
                request.WasEmptyNet,
                request.Description);

            HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Updated goal {GoalEventId} on match {MatchId}",
                request.GoalEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while updating the goal.", ex.Flatten());
        }
    }
}

/// <summary>
/// Corrects a penalty event during live match operations.
/// </summary>
public class UpdateHockeyPenaltyHandler : IRequestHandler<UpdateHockeyPenaltyCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyPenaltyHandler> _logger;

    public UpdateHockeyPenaltyHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyPenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        UpdateHockeyPenaltyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            match.UpdatePenaltyEvent(
                request.PenaltyEventId,
                request.PenaltyMatchTeamId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.Severity,
                request.Offence,
                request.PenaltyMinutes,
                request.PenalizedActivePlayerId,
                request.ServedByActivePlayerId,
                request.IsBenchPenalty,
                request.Description);

            HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Updated penalty {PenaltyEventId} on match {MatchId}",
                request.PenaltyEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyPenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while updating the penalty.", ex.Flatten());
        }
    }
}

/// <summary>
/// Corrects a shot event during live match operations.
/// </summary>
public class UpdateHockeyShotHandler : IRequestHandler<UpdateHockeyShotCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyShotHandler> _logger;

    public UpdateHockeyShotHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyShotHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        UpdateHockeyShotCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            match.UpdateShotEvent(
                request.ShotEventId,
                request.ShootingMatchTeamId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.ShotResult,
                request.CountsAsShotOnGoal,
                request.ShooterActivePlayerId,
                request.GoalieActivePlayerId,
                request.Description);

            HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Updated shot {ShotEventId} on match {MatchId}",
                request.ShotEventId,
                request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while updating the shot.", ex.Flatten());
        }
    }
}
