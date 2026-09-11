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

public class RemoveTeamFromSeasonHandler : IRequestHandler<RemoveTeamFromSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromSeasonHandler> _logger;

    public RemoveTeamFromSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(RemoveTeamFromSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);

            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (season == null)
            {
                _logger.LogWarning("Season with ID {SeasonId} not found", request.CompetitionId);
                return Result<FootballSeasonDto>.Failure($"Season with ID {request.CompetitionId} not found");
            }

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team with ID {TeamId} not found", request.TeamId);
                return Result<FootballSeasonDto>.Failure($"Team with ID {request.TeamId} not found");
            }

            season.RemoveTeam(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully removed team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);

            Dictionary<Guid, Club> clubs = new();
            foreach (FootballTeam seasonTeam in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(seasonTeam.ClubId);
                if (club != null)
                {
                    clubs[seasonTeam.ClubId] = club;
                }
            }

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);

            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos, clubs);
            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);
            return Result<FootballSeasonDto>.Failure($"An error occurred while removing team from season: {ex.Message}");
        }
    }
}
