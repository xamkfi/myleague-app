using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles recording a missed penalty shot on a hockey match.
/// </summary>
public class RecordHockeyMissedPenaltyShotHandler
    : IRequestHandler<RecordHockeyMissedPenaltyShotCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;

    public RecordHockeyMissedPenaltyShotHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        RecordHockeyMissedPenaltyShotCommand request,
        CancellationToken cancellationToken)
    {
        HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

        try
        {
            HockeyMissedPenaltyShot missed = new(
                match.Id,
                request.MatchTeamId,
                request.ShooterActivePlayerId,
                request.PeriodNumber,
                TimeSpan.FromSeconds(request.TimeInSeconds));
            match.AddEvent(missed);
            _matchRepository.MarkEventAsAdded(missed);
        }
        catch (InvalidOperationException ex)
        {
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }

        HockeyDomainValidationResult validation = HockeyMatchValidationService.ValidateEventPlayerReferences(match);
        if (!validation.IsValid)
            return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
    }
}

/// <summary>
/// Handles deleting a missed penalty shot from a hockey match.
/// </summary>
public class DeleteHockeyMissedPenaltyShotHandler
    : IRequestHandler<DeleteHockeyMissedPenaltyShotCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;

    public DeleteHockeyMissedPenaltyShotHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        DeleteHockeyMissedPenaltyShotCommand request,
        CancellationToken cancellationToken)
    {
        HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

        try
        {
            HockeyMissedPenaltyShot deleted = match.DeleteMissedPenaltyShotEvent(request.MissedPenaltyShotEventId);
            _matchRepository.MarkEventAsDeleted(deleted);
        }
        catch (InvalidOperationException ex)
        {
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
    }
}
