using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

/// <summary>
/// Handles <see cref="ChangeFootballSeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeFootballSeasonTeamCategoryHandler : IRequestHandler<ChangeFootballSeasonTeamCategoryCommand, Result>
{
    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeFootballSeasonTeamCategoryHandler> _logger;

    public ChangeFootballSeasonTeamCategoryHandler(
        IFootballCompetitionRepository competitionRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<ChangeFootballSeasonTeamCategoryHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeFootballSeasonTeamCategoryCommand request, CancellationToken cancellationToken)
    {
        FootballCompetition? competition = await _competitionRepository.GetByIdAsync(request.SeasonId);
        if (competition is not FootballSeason season)
        {
            return Result.NotFound("FootballSeason", request.SeasonId);
        }

        season.UpdateTeamCategory(request.TeamCategory);
        await _competitionRepository.UpdateAsync(season);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Moved football season {SeasonId} to team category {TeamCategory}",
            request.SeasonId,
            request.TeamCategory);
        return Result.Success();
    }
}
