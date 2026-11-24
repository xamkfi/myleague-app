using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using MediatR;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles correcting the phase of play that decided a hockey match.
/// </summary>
public class SetHockeyMatchDecisionHandler : IRequestHandler<SetHockeyMatchDecisionCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;

    public SetHockeyMatchDecisionHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        SetHockeyMatchDecisionCommand request,
        CancellationToken cancellationToken)
    {
        HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

        try
        {
            foreach (HockeyMatchEvent removed in match.SetDecidedIn(request.Decision))
            {
                _matchRepository.MarkEventAsDeleted(removed);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
    }
}
