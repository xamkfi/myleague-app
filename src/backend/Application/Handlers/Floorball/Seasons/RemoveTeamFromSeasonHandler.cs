using MediatR;
using Microsoft.Extensions.Logging;
using Application.Commands.Floorball.Season;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;

namespace Application.Handlers.Floorball.Seasons;

public class RemoveTeamFromSeasonHandler : IRequestHandler<RemoveTeamFromSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromSeasonHandler> _logger;

    public RemoveTeamFromSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
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
            _logger.LogInformation("Removing team {TeamId} from season {SeasonId}", request.TeamId, request.SeasonId);

            // Get the season
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.SeasonId);
            if (season == null)
            {
                _logger.LogWarning("Season with ID {SeasonId} not found", request.SeasonId);
                return Result<FloorballSeasonDto>.Failure($"Season with ID {request.SeasonId} not found");
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

            _logger.LogInformation("Successfully removed team {TeamId} from season {SeasonId}", request.TeamId, request.SeasonId);

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

            FloorballSeasonDto seasonDto = await FloorballSeasonMapper.ToDtoAsync(season, _seasonDivisionRepository, clubs);
            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from season {SeasonId}", request.TeamId, request.SeasonId);
            return Result<FloorballSeasonDto>.Failure($"An error occurred while removing team from season: {ex.Message}");
        }
    }
} 