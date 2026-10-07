using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles <see cref="ChangeHockeySeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeHockeySeasonTeamCategoryHandler : IRequestHandler<ChangeHockeySeasonTeamCategoryCommand, Result>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeHockeySeasonTeamCategoryHandler> _logger;

    public ChangeHockeySeasonTeamCategoryHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ChangeHockeySeasonTeamCategoryHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeHockeySeasonTeamCategoryCommand request, CancellationToken cancellationToken)
    {
        HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
        if (season is null)
        {
            return Result.NotFound("HockeySeason", request.SeasonId);
        }

        season.UpdateTeamCategory(request.TeamCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Moved hockey season {SeasonId} to team category {TeamCategory}",
            request.SeasonId,
            request.TeamCategory);
        return Result.Success();
    }
}
