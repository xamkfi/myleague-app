using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Handler for correcting the phase of play that decided a football match
/// </summary>
public class SetFootballMatchDecisionHandler : IRequestHandler<SetFootballMatchDecisionCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;

    public SetFootballMatchDecisionHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
    }

    public async Task<Result<FootballMatchDto>> Handle(SetFootballMatchDecisionCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);

        try
        {
            match.SetDecidedIn(request.Decision);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notificationSenderService.SendNotificationAsync(
            FootballNotificationEvents.GoalScored,
            new MatchNotificationPayload(match.Id));

        return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
    }
}
