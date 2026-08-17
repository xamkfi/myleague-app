using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class DeactivateFootballSeasonHandler : IRequestHandler<DeactivateFootballSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<DeactivateFootballSeasonHandler> _logger;

    public DeactivateFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<DeactivateFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(DeactivateFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FootballSeasonDto>.Failure($"Season with ID {request.Id} not found.");
            }

            _logger.LogInformation("Deactivating football season: {SeasonId}", request.Id);
            season.Deactivate();
            await _seasonRepository.UpdateAsync(season);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Dictionary<Guid, Club> clubsDict = new();
            foreach (FootballTeam team in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club != null)
                {
                    clubsDict[team.ClubId] = club;
                }
            }

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully deactivated football season: {SeasonId}", request.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure("An error occurred while deactivating the season.");
        }
    }
}
