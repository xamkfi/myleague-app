using Application.Common;
using Application.Constants;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for recording a missed penalty shot in a floorball match
/// </summary>
public class RecordFloorballMissedPenaltyHandler : IRequestHandler<RecordFloorballMissedPenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;

    public RecordFloorballMissedPenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordFloorballMissedPenaltyCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);

        // Loading the team includes its roster, which the domain validates the shooter against.
        FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
        if (team == null)
            return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.TeamId);

        FloorballPlayer? shooter = await _playerRepository.GetByIdAsync(request.ShooterPlayerId);
        if (shooter == null)
            return Result<FloorballMatchDto>.NotFound("FloorballPlayer", request.ShooterPlayerId);

        try
        {
            FloorballMissedPenalty missedPenalty = match.RecordMissedPenalty(
                team, shooter, request.PeriodNumber, request.TimeInSeconds);
            _matchRepository.MarkEventAsAdded(missedPenalty);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notificationSenderService.SendNotificationAsync(
            FloorballNotificationEvents.PenaltyAssigned,
            new MatchNotificationPayload(match.Id));

        return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
    }
}

/// <summary>
/// Handler for deleting a missed penalty shot from a floorball match
/// </summary>
public class DeleteFloorballMissedPenaltyHandler : IRequestHandler<DeleteFloorballMissedPenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;

    public DeleteFloorballMissedPenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeleteFloorballMissedPenaltyCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);

        try
        {
            match.DeleteMissedPenaltyEvent(request.MissedPenaltyEventId);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
    }
}
