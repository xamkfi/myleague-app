using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles adding a Common Division to a hockey season via Domain <c>AddDivision</c>.
/// </summary>
public class AddDivisionToHockeySeasonHandler
    : IRequestHandler<AddDivisionToHockeySeasonCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddDivisionToHockeySeasonHandler> _logger;

    public AddDivisionToHockeySeasonHandler(
        IHockeyCompetitionRepository competitionRepository,
        IDivisionRepository divisionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddDivisionToHockeySeasonHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(
        AddDivisionToHockeySeasonCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonByIdAsync(request.SeasonId);
            if (season is null)
            {
                return Result<HockeySeasonDto>.NotFound("HockeySeason", request.SeasonId);
            }

            Division? division = await _divisionRepository.GetByIdAsync(request.DivisionId);
            if (division is null)
            {
                return Result<HockeySeasonDto>.NotFound("Division", request.DivisionId);
            }

            if (division.SportType != SportsCategory.Icehockey)
            {
                return Result<HockeySeasonDto>.Failure("Division must be an ice hockey division.");
            }

            season.AddDivision(request.DivisionId, request.Name, request.SortOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added division {DivisionId} to hockey season {SeasonId}",
                request.DivisionId,
                request.SeasonId);

            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddDivisionToHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddDivisionToHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddDivisionToHockeySeason for season {SeasonId}", request.SeasonId);
            return Result<HockeySeasonDto>.Failure("An error occurred while adding the division to the season.", ex.Flatten());
        }
    }
}
