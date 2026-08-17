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
/// Handles recording a video review on a hockey match.
/// </summary>
public class RecordHockeyVideoReviewHandler
    : IRequestHandler<RecordHockeyVideoReviewCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyVideoReviewHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public RecordHockeyVideoReviewHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyVideoReviewHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        RecordHockeyVideoReviewCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyVideoReview review = new(
                match.Id,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.ReviewType,
                request.OriginalDecision,
                request.FinalDecision,
                request.IsCoachChallenge,
                request.WasSuccessful,
                request.RequestedByMatchTeamId,
                request.Description);

            match.AddEvent(review);
            _matchRepository.MarkEventAsAdded(review);

            HockeyDomainValidationResult validation = _validationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recorded video review on match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyVideoReview for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyVideoReview for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyVideoReview for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while recording the video review.", ex.Flatten());
        }
    }
}
