using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Handler for recording a missed penalty kick in a football match
/// </summary>
public class RecordFootballMissedPenaltyHandler : IRequestHandler<RecordFootballMissedPenaltyCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;

    public RecordFootballMissedPenaltyHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
    }

    public async Task<Result<FootballMatchDto>> Handle(RecordFootballMissedPenaltyCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);

        // Loading the team includes its roster, which the domain validates the shooter against.
        FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
        if (team == null)
            return Result<FootballMatchDto>.NotFound("FootballTeam", request.TeamId);

        FootballPlayer? shooter = await _playerRepository.GetByIdAsync(request.ShooterPlayerId);
        if (shooter == null)
            return Result<FootballMatchDto>.NotFound("FootballPlayer", request.ShooterPlayerId);

        try
        {
            FootballMissedPenalty missedPenalty = match.RecordMissedPenalty(
                team, shooter, request.PeriodNumber, request.TimeInSeconds);
            _matchRepository.MarkEventAsAdded(missedPenalty);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notificationSenderService.SendNotificationAsync(
            FootballNotificationEvents.CardAssigned,
            new MatchNotificationPayload(match.Id));

        return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
    }
}

/// <summary>
/// Handler for deleting a missed penalty kick from a football match
/// </summary>
public class DeleteFootballMissedPenaltyHandler : IRequestHandler<DeleteFootballMissedPenaltyCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;

    public DeleteFootballMissedPenaltyHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FootballMatchDto>> Handle(DeleteFootballMissedPenaltyCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);

        try
        {
            match.DeleteMissedPenaltyEvent(request.MissedPenaltyEventId);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
    }
}
