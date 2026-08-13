using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class CreateFootballSeasonHandler : IRequestHandler<CreateFootballSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFootballSeasonHandler> _logger;

    public CreateFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CreateFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(CreateFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballSeason season = FootballSeasonMapper.ToEntity(request);

            _logger.LogInformation("Creating new football season: {Name}", request.Name);
            await _seasonRepository.AddAsync(season);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (Guid divisionId in request.DivisionIds)
            {
                await _seasonDivisionRepository.AddCompetitionDivisionAsync(season.Id, divisionId);
                _logger.LogInformation("Added division {DivisionId} to season {SeasonId}", divisionId, season.Id);
            }

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos);
            _logger.LogInformation("Successfully created football season with ID: {SeasonId}", season.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football season: {Name}", request.Name);
            return Result<FootballSeasonDto>.Failure("An error occurred while creating the football season.");
        }
    }
}
