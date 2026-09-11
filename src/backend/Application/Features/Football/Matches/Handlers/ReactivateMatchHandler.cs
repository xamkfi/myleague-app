using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class ReactivateMatchHandler : IRequestHandler<ReactivateMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<ReactivateMatchHandler> _logger;

    public ReactivateMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<ReactivateMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(ReactivateMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.Reactivate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reactivate match {MatchId}: {Message}", request.MatchId, ex.Message);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reactivating match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while reactivating the match.");
        }
    }
}
