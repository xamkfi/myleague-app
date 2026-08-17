using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class CompleteFootballSeasonHandler : IRequestHandler<CompleteFootballSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteFootballSeasonHandler> _logger;

    public CompleteFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CompleteFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(CompleteFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FootballSeasonDto>.Failure($"Season with ID {request.Id} not found.");
            }

            _logger.LogInformation("Completing football season: {SeasonId}", request.Id);
            season.Complete();
            await _seasonRepository.UpdateAsync(season);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos);
            _logger.LogInformation("Successfully completed football season: {SeasonId}", request.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure("An error occurred while completing the season.");
        }
    }
}
