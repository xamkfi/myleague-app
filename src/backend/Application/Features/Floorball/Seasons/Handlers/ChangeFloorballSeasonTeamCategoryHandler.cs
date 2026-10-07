using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Domain.Entities.Floorball.Competitions;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handles <see cref="ChangeFloorballSeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeFloorballSeasonTeamCategoryHandler : IRequestHandler<ChangeFloorballSeasonTeamCategoryCommand, Result>
{
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeFloorballSeasonTeamCategoryHandler> _logger;

    public ChangeFloorballSeasonTeamCategoryHandler(
        IFloorballCompetitionRepository competitionRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<ChangeFloorballSeasonTeamCategoryHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeFloorballSeasonTeamCategoryCommand request, CancellationToken cancellationToken)
    {
        FloorballCompetition? competition = await _competitionRepository.GetByIdAsync(request.SeasonId);
        if (competition is not FloorballSeason season)
        {
            return Result.NotFound("FloorballSeason", request.SeasonId);
        }

        season.UpdateTeamCategory(request.TeamCategory);
        await _competitionRepository.UpdateAsync(season);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Moved floorball season {SeasonId} to team category {TeamCategory}",
            request.SeasonId,
            request.TeamCategory);
        return Result.Success();
    }
}
