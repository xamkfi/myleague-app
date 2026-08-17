using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles recording a goal on a hockey match.
/// </summary>
public class RecordHockeyGoalHandler : IRequestHandler<RecordHockeyGoalCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyGoalHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public RecordHockeyGoalHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(RecordHockeyGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyGoal goal = new(
                match.Id,
                request.ScoringMatchTeamId,
                request.ScorerActivePlayerId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.GoalStrength,
                request.PrimaryAssistActivePlayerId,
                request.SecondaryAssistActivePlayerId,
                request.GoalieActivePlayerId,
                wasEmptyNet: request.WasEmptyNet,
                description: request.Description);

            match.AddEvent(goal);
            _matchRepository.MarkEventAsAdded(goal);

            HockeyDomainValidationResult validation = _validationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recorded goal on match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyGoal for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while recording the goal.", ex.Flatten());
        }
    }
}
