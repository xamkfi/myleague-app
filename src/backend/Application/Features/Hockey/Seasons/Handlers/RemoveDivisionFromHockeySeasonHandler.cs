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
/// Handles RemoveDivisionFromHockeySeason.
/// </summary>
public class RemoveDivisionFromHockeySeasonHandler
    : IRequestHandler<RemoveDivisionFromHockeySeasonCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveDivisionFromHockeySeasonHandler> _logger;

    public RemoveDivisionFromHockeySeasonHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveDivisionFromHockeySeasonHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        RemoveDivisionFromHockeySeasonCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            season.RemoveDivision(request.CompetitionDivisionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed division {CompetitionDivisionId} from hockey season {SeasonId}",
                request.CompetitionDivisionId,
                request.SeasonId);

            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveDivisionFromHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RemoveDivisionFromHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveDivisionFromHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while removing the division from the season.", ex.Flatten());
        }
    }
}
