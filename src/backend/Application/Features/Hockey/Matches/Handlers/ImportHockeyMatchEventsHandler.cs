using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Records a batch of historical goals and penalties on a started hockey match
/// in one unit of work. Season statistics are rebuilt on match finish, as with live events.
/// </summary>
public class ImportHockeyMatchEventsHandler
    : IRequestHandler<ImportHockeyMatchEventsCommand, Result<HockeyMatchEventsImportDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ImportHockeyMatchEventsHandler> _logger;

    public ImportHockeyMatchEventsHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ImportHockeyMatchEventsHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchEventsImportDto>> Handle(
        ImportHockeyMatchEventsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchEventsImportDto>.NotFound("HockeyMatch", request.MatchId);
            }

            if (match.Status is HockeyMatchStatus.Finished
                or HockeyMatchStatus.Cancelled
                or HockeyMatchStatus.Postponed
                or HockeyMatchStatus.Forfeit)
            {
                return Result<HockeyMatchEventsImportDto>.Failure(
                    $"Match must be in progress to import events. Current status: {match.Status}");
            }

            int goalsRecorded = 0;
            int penaltiesRecorded = 0;
            List<string> eventErrors = new();

            for (int index = 0; index < request.Events.Count; index++)
            {
                ImportHockeyMatchEventItem item = request.Events[index];
                try
                {
                    if (string.Equals(item.EventType, "Goal", StringComparison.OrdinalIgnoreCase))
                    {
                        RecordImportedGoal(match, item);
                        goalsRecorded++;
                    }
                    else if (string.Equals(item.EventType, "Penalty", StringComparison.OrdinalIgnoreCase))
                    {
                        RecordImportedPenalty(match, item);
                        penaltiesRecorded++;
                    }
                    else
                    {
                        eventErrors.Add($"[{index}] Unknown event type '{item.EventType}'.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    eventErrors.Add($"[{index}] {item.EventType}: {ex.Message}");
                    _logger.LogWarning(
                        ex,
                        "Skipped import event {Index} of type {EventType} on match {MatchId}",
                        index,
                        item.EventType,
                        request.MatchId);
                }
                catch (ArgumentException ex)
                {
                    eventErrors.Add($"[{index}] {item.EventType}: {ex.Message}");
                    _logger.LogWarning(
                        ex,
                        "Skipped import event {Index} of type {EventType} on match {MatchId}",
                        index,
                        item.EventType,
                        request.MatchId);
                }
            }

            HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchEventsImportDto>.Failure(
                    string.Join(" ", validation.Errors),
                    validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            HockeyMatchEventsImportDto dto = new(
                HockeyMatchMapper.ToDto(match),
                goalsRecorded,
                penaltiesRecorded,
                eventErrors);

            _logger.LogInformation(
                "Imported {GoalCount} goals and {PenaltyCount} penalties on hockey match {MatchId} ({ErrorCount} event errors)",
                goalsRecorded,
                penaltiesRecorded,
                request.MatchId,
                eventErrors.Count);

            return Result<HockeyMatchEventsImportDto>.Success(dto);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed importing events for hockey match {MatchId}", request.MatchId);
            string detail = ex.InnerException?.Message ?? ex.Message;
            return Result<HockeyMatchEventsImportDto>.Failure(detail, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Failed importing events for hockey match {MatchId}", request.MatchId);
            string detail = ex.InnerException?.Message ?? ex.Message;
            return Result<HockeyMatchEventsImportDto>.Failure(detail, ex.Flatten());
        }
    }

    private void RecordImportedGoal(HockeyMatch match, ImportHockeyMatchEventItem item)
    {
        if (!item.ActivePlayerId.HasValue)
        {
            throw new ArgumentException("Scorer active player is required for a goal.");
        }

        HockeyGoal goal = new(
            match.Id,
            item.MatchTeamId,
            item.ActivePlayerId.Value,
            item.PeriodNumber,
            TimeSpan.FromSeconds(item.TimeInSeconds),
            item.GoalStrength ?? HockeyGoalStrength.EvenStrength,
            item.PrimaryAssistActivePlayerId,
            item.SecondaryAssistActivePlayerId,
            item.GoalieActivePlayerId,
            wasEmptyNet: item.WasEmptyNet,
            description: item.Description);

        match.AddEvent(goal);
        _matchRepository.MarkEventAsAdded(goal);
    }

    private void RecordImportedPenalty(HockeyMatch match, ImportHockeyMatchEventItem item)
    {
        bool isBench = item.IsBenchPenalty || !item.ActivePlayerId.HasValue;
        HockeyPenalty penalty = new(
            match.Id,
            item.MatchTeamId,
            item.PeriodNumber,
            TimeSpan.FromSeconds(item.TimeInSeconds),
            item.Severity ?? HockeyPenaltySeverity.Minor,
            item.Offence ?? HockeyPenaltyOffence.UnsportsmanlikeConduct,
            item.PenaltyMinutes ?? 2,
            item.ActivePlayerId,
            item.ServedByActivePlayerId,
            isBenchPenalty: isBench,
            description: item.Description);

        match.AddEvent(penalty);
        _matchRepository.MarkEventAsAdded(penalty);
    }
}
