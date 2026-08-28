using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles retrieving active hockey seasons.
/// </summary>
public class GetActiveHockeySeasonsHandler
    : IRequestHandler<GetActiveHockeySeasonsQuery, Result<IEnumerable<HockeySeasonDto>>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetActiveHockeySeasonsHandler> _logger;

    public GetActiveHockeySeasonsHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetActiveHockeySeasonsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeySeasonDto>>> Handle(
        GetActiveHockeySeasonsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeySeason> seasons = await _competitionRepository.GetAllSeasonsAsync();
            List<HockeySeasonDto> active = seasons
                .Where(s => s.IsActive)
                .Where(s => request.TeamCategory is null || s.TeamCategory == request.TeamCategory)
                .Select(HockeyCompetitionMapper.ToSeasonDto)
                .ToList();

            return Result<IEnumerable<HockeySeasonDto>>.Success(active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active hockey seasons");
            return Result<IEnumerable<HockeySeasonDto>>.Failure(
                "An error occurred while retrieving active hockey seasons.",
                ex.Flatten());
        }
    }
}
