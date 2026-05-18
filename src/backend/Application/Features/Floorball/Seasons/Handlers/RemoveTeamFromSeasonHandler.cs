using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Floorball.Seasons.Commands;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;

namespace Application.Features.Floorball.Seasons.Handlers;

public class RemoveTeamFromSeasonHandler : IRequestHandler<RemoveTeamFromSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromSeasonHandler> _logger;

    public RemoveTeamFromSeasonHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballSeasonDto>> Handle(RemoveTeamFromSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);

            // Get the season
            FloorballCompetition? season = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (season == null)
            {
                _logger.LogWarning("Season with ID {SeasonId} not found", request.CompetitionId);
                return Result<FloorballSeasonDto>.Failure($"Season with ID {request.CompetitionId} not found");
            }

            // Get the team
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team with ID {TeamId} not found", request.TeamId);
                return Result<FloorballSeasonDto>.Failure($"Team with ID {request.TeamId} not found");
            }

            // Remove team from season using domain method
            season.RemoveTeam(team);

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully removed team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);

            // Load clubs for all teams in the season for the DTO mapping
            Dictionary<Guid, Club> clubs = new Dictionary<Guid, Club>();
            foreach (FloorballTeam seasonTeam in season.Teams)
            {
                Domain.Entities.Common.Club? club = await _clubRepository.GetByIdAsync(seasonTeam.ClubId);
                if (club != null)
                {
                    clubs[seasonTeam.ClubId] = club;
                }
            }

            IEnumerable<FloorballCompetitionDivision> seasonDivisions = await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season, seasonDivisionDtos, clubs);
            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from season {SeasonId}", request.TeamId, request.CompetitionId);
            return Result<FloorballSeasonDto>.Failure($"An error occurred while removing team from season: {ex.Message}");
        }
    }
} 
