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
/// Handles recording a shootout attempt on a hockey match.
/// </summary>
public class RecordHockeyShootoutAttemptHandler
    : IRequestHandler<RecordHockeyShootoutAttemptCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyShootoutAttemptHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public RecordHockeyShootoutAttemptHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyShootoutAttemptHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        RecordHockeyShootoutAttemptCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyShootoutAttempt attempt = new(
                match.Id,
                request.MatchTeamId,
                request.ShooterActivePlayerId,
                request.GoalieActivePlayerId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.ShotOrder,
                request.Result,
                request.Description);

            match.AddEvent(attempt);
            _matchRepository.MarkEventAsAdded(attempt);

            HockeyDomainValidationResult validation = _validationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recorded shootout attempt on match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyShootoutAttempt for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyShootoutAttempt for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyShootoutAttempt for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while recording the shootout attempt.", ex.Flatten());
        }
    }
}
