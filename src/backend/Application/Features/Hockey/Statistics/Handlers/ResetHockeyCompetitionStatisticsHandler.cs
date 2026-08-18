using Application.Common;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Deletes competition aggregate statistics without rebuilding.
/// </summary>
public class ResetHockeyCompetitionStatisticsHandler
    : IRequestHandler<ResetHockeyCompetitionStatisticsCommand, Result>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ResetHockeyCompetitionStatisticsHandler> _logger;

    public ResetHockeyCompetitionStatisticsHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyStatisticsRepository statisticsRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ResetHockeyCompetitionStatisticsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ResetHockeyCompetitionStatisticsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Scope is not null)
            {
                HockeyStatisticsHandlerSupport.ValidateScopeIds(
                    request.Scope.Value,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);
            }

            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
                return Result.NotFound("HockeyCompetition", request.CompetitionId);

            await _statisticsRepository.ResetCompetitionStatisticsAsync(
                request.CompetitionId,
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed ResetHockeyCompetitionStatistics for {CompetitionId}",
                request.CompetitionId);
            return Result.Failure("An error occurred while resetting competition statistics.", ex.Flatten());
        }
    }
}
