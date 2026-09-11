using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class RecordPenaltyShootoutHandler : IRequestHandler<RecordPenaltyShootoutCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RecordPenaltyShootoutHandler> _logger;

    public RecordPenaltyShootoutHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RecordPenaltyShootoutHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(RecordPenaltyShootoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.RecordPenaltyShootout();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording penalty shootout for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while recording the penalty shootout.");
        }
    }
}
