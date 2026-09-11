using Application.Common;
using Application.Features.Football.Matches.Commands;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class DeleteFootballMatchHandler : IRequestHandler<DeleteFootballMatchCommand, Result>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballMatchHandler> _logger;

    public DeleteFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Attempt to delete non-existent football match with ID: {MatchId}", request.MatchId);
                return Result.NotFound("FootballMatch", request.MatchId);
            }

            if (match.Status != FootballMatchStatus.Scheduled)
            {
                _logger.LogWarning(
                    "Refusing to delete football match {MatchId} because its status is {Status} (only Scheduled may be deleted).",
                    request.MatchId,
                    match.Status);
                return Result.Failure(
                    $"Only matches in the Scheduled state can be deleted (current status: {match.Status}).");
            }

            _logger.LogInformation("Deleting football match with ID: {MatchId}", request.MatchId);
            await _matchRepository.DeleteAsync(match.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football match {MatchId}", request.MatchId);
            return Result.Failure("An error occurred while deleting the football match.", ex.Flatten());
        }
    }
}
