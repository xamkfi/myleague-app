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
/// Handles recording a shot on a hockey match.
/// </summary>
public class RecordHockeyShotHandler : IRequestHandler<RecordHockeyShotCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecordHockeyShotHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public RecordHockeyShotHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecordHockeyShotHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(RecordHockeyShotCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyShot shot = new(
                match.Id,
                request.ShootingMatchTeamId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds),
                request.ShotResult,
                request.CountsAsShotOnGoal,
                request.ShooterActivePlayerId,
                request.GoalieActivePlayerId,
                description: request.Description);

            match.AddEvent(shot);
            _matchRepository.MarkEventAsAdded(shot);

            HockeyDomainValidationResult validation = _validationService.ValidateEventPlayerReferences(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recorded shot on match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecordHockeyShot for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while recording the shot.", ex.Flatten());
        }
    }
}
