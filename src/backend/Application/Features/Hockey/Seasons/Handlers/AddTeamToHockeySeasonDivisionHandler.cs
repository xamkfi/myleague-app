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
/// Handles AddTeamToHockeySeasonDivision.
/// </summary>
public class AddTeamToHockeySeasonDivisionHandler
    : IRequestHandler<AddTeamToHockeySeasonDivisionCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToHockeySeasonDivisionHandler> _logger;

    public AddTeamToHockeySeasonDivisionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddTeamToHockeySeasonDivisionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        AddTeamToHockeySeasonDivisionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            season.AddTeamToDivision(request.CompetitionDivisionId, request.CompetitionTeamId, request.Seed);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added competition team {CompetitionTeamId} to division {CompetitionDivisionId} on season {SeasonId}",
                request.CompetitionTeamId,
                request.CompetitionDivisionId,
                request.SeasonId);

            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddTeamToHockeySeasonDivision for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddTeamToHockeySeasonDivision for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddTeamToHockeySeasonDivision for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while adding the team to the division.", ex.Flatten());
        }
    }
}
