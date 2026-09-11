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
/// Handles retrieving a hockey season by id.
/// </summary>
public class GetHockeySeasonByIdHandler : IRequestHandler<GetHockeySeasonByIdQuery, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetHockeySeasonByIdHandler> _logger;

    public GetHockeySeasonByIdHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetHockeySeasonByIdHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(GetHockeySeasonByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.Id);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.Id);
            }

            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey season {SeasonId}", request.Id);
            return Result<HockeySeasonDto>.Failure("An error occurred while retrieving the hockey season.", ex.Flatten());
        }
    }
}
