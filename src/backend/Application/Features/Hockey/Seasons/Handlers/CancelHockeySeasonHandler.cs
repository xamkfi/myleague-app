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
/// Handles CancelHockeySeason.
/// </summary>
public class CancelHockeySeasonHandler : IRequestHandler<CancelHockeySeasonCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CancelHockeySeasonHandler> _logger;

    public CancelHockeySeasonHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CancelHockeySeasonHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        CancelHockeySeasonCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            season.Cancel();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CancelHockeySeason completed for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected CancelHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CancelHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed CancelHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while cancelling the hockey season.", ex.Flatten());
        }
    }
}
