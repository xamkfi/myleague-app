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
/// Handles recording a goalie change on a hockey match.
/// </summary>
public class RecordHockeyGoalieChangeHandler : IRequestHandler<RecordHockeyGoalieChangeCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyGoalieChangeHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public RecordHockeyGoalieChangeHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyGoalieChangeHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(RecordHockeyGoalieChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyGoalieChange change = new(
                match.Id,
                request.MatchTeamId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.OutgoingGoalieActivePlayerId,
                request.IncomingGoalieActivePlayerId,
                request.Reason,
                request.Description);

            match.AddEvent(change);
            _matchRepository.MarkEventAsAdded(change);

            HockeyDomainValidationResult validation = _validationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recorded goalie change on match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyGoalieChange for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyGoalieChange for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyGoalieChange for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while recording the goalie change.", ex.Flatten());
        }
    }
}
