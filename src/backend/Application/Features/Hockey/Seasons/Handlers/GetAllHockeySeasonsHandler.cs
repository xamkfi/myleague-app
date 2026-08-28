using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Queries;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles retrieving all hockey seasons.
/// </summary>
public class GetAllHockeySeasonsHandler : IRequestHandler<GetAllHockeySeasonsQuery, Result<IEnumerable<HockeySeasonDto>>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetAllHockeySeasonsHandler> _logger;

    public GetAllHockeySeasonsHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetAllHockeySeasonsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeySeasonDto>>> Handle(GetAllHockeySeasonsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Domain.Entities.Hockey.Competitions.HockeySeason> seasons =
                await _competitionRepository.GetAllSeasonsAsync();
            IEnumerable<HockeySeasonDto> dtos = seasons
                .Where(season => request.TeamCategory is null || season.TeamCategory == request.TeamCategory)
                .Select(HockeyCompetitionMapper.ToSeasonDto);
            return Result<IEnumerable<HockeySeasonDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list hockey seasons");
            return Result<IEnumerable<HockeySeasonDto>>.Failure(
                "An error occurred while retrieving hockey seasons.",
                ex.Flatten());
        }
    }
}
