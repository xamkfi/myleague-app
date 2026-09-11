using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class DeleteCardHandler : IRequestHandler<DeleteCardCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCardHandler> _logger;

    public DeleteCardHandler(
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteCardHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FootballCard deletedCard = match.DeleteCardEvent(request.CardEventId);
            await RecordCardHandler.UpdatePlayerCardStatistics(
                deletedCard,
                match.CompetitionId,
                increment: false,
                _statisticsRepository,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting card {CardId} from match {MatchId}", request.CardEventId, request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
    }
}
