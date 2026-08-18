using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Hockey.Matches;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles setting the playoff schedule on a hockey season.
/// </summary>
public class SetHockeySeasonPlayoffScheduleHandler
    : IRequestHandler<SetHockeySeasonPlayoffScheduleCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeySeasonPlayoffScheduleHandler> _logger;

    public SetHockeySeasonPlayoffScheduleHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeySeasonPlayoffScheduleHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        SetHockeySeasonPlayoffScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            List<HockeyPlayoffScheduleSlot> slots = request.Slots.Select(s => new HockeyPlayoffScheduleSlot(
                s.Round,
                s.SeriesOrder,
                s.MatchOrder,
                s.HomeSourceType,
                s.AwaySourceType,
                s.HomeSourceGroupId,
                s.AwaySourceGroupId,
                s.HomeSourceSeriesId,
                s.AwaySourceSeriesId,
                s.HomeSourceRank,
                s.AwaySourceRank,
                s.ManualHomeCompetitionTeamId,
                s.ManualAwayCompetitionTeamId)).ToList();

            season.SetPlayoffSchedule(slots);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Set playoff schedule on season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected playoff schedule for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid playoff schedule for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed playoff schedule for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while setting the playoff schedule.", ex.Flatten());
        }
    }
}
