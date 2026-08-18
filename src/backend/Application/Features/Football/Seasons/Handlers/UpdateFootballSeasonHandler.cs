using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class UpdateFootballSeasonHandler : IRequestHandler<UpdateFootballSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballSeasonHandler> _logger;

    public UpdateFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(UpdateFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? existingSeason = await _seasonRepository.GetByIdAsync(request.Id);
            if (existingSeason == null)
            {
                _logger.LogWarning("Attempt to update non-existent football season with ID: {SeasonId}", request.Id);
                return Result<FootballSeasonDto>.NotFound("FootballSeason", request.Id);
            }

            FootballSeasonMapper.UpdateFromCommand(existingSeason, request);

            _logger.LogInformation("Updating football season: {SeasonId}", existingSeason.Id);
            await _seasonRepository.UpdateAsync(existingSeason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(existingSeason.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);

            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(existingSeason, seasonDivisionDtos);
            _logger.LogInformation("Successfully updated football season with ID: {SeasonId}", existingSeason.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure("An error occurred while updating the football season.");
        }
    }
}
