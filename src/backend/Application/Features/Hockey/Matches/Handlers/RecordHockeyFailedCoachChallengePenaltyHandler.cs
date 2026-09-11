using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles recording a failed coach-challenge penalty linked to a video review.
/// </summary>
public class RecordHockeyFailedCoachChallengePenaltyHandler
    : IRequestHandler<RecordHockeyFailedCoachChallengePenaltyCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyFailedCoachChallengePenaltyHandler> _logger;

    public RecordHockeyFailedCoachChallengePenaltyHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyFailedCoachChallengePenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        RecordHockeyFailedCoachChallengePenaltyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyVideoReview? review = match.Events.OfType<HockeyVideoReview>()
                .FirstOrDefault(e => e.Id == request.VideoReviewId);
            if (review is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyVideoReview", request.VideoReviewId);
            }

            HockeyCoachChallengeRules rules = new(
                request.Enabled,
                request.MaxChallengesPerTeam,
                request.LoseChallengeAfterFailed,
                request.PenaltyForFailedChallenge,
                request.FailedChallengePenaltyMinutes,
                request.FailedChallengePenaltyOffence,
                request.FailedChallengePenaltySeverity,
                request.AllowChallengeInOvertime,
                request.AllowChallengeInShootout);

            HockeyPenalty penalty = match.RecordFailedCoachChallengePenalty(
                review,
                rules,
                request.PenaltyMatchTeamId);
            _matchRepository.MarkEventAsAdded(penalty);

            HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Recorded failed coach-challenge penalty on match {MatchId} for review {ReviewId}",
                request.MatchId,
                request.VideoReviewId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyFailedCoachChallengePenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyFailedCoachChallengePenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyFailedCoachChallengePenalty for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(
                "An error occurred while recording the failed coach-challenge penalty.",
                ex.Flatten());
        }
    }
}
