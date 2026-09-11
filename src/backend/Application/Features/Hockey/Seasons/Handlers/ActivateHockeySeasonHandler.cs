using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles ActivateHockeySeason.
/// </summary>
public class ActivateHockeySeasonHandler : IRequestHandler<ActivateHockeySeasonCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateHockeySeasonHandler> _logger;

    public ActivateHockeySeasonHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ActivateHockeySeasonHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        ActivateHockeySeasonCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            season.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ActivateHockeySeason completed for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected ActivateHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid ActivateHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed ActivateHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while activating the hockey season.", ex.Flatten());
        }
    }
}
