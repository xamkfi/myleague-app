using Application.Common;
using Application.Features.Common.ClubAdmin.DTOs;
using Application.Features.Common.ClubAdmin.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.ClubAdmin.Handlers;

/// <summary>
/// Handler that resolves all clubs the person actively manages via the club manager link
/// entities, together with the floorball, football, and hockey teams under each club.
/// </summary>
public class GetMyClubsHandler : IRequestHandler<GetMyClubsQuery, Result<IEnumerable<ClubAdminClubDto>>>
{
    private readonly IClubManagerRepository _clubManagerRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IHockeyTeamRepository _hockeyTeamRepository;
    private readonly ILogger<GetMyClubsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetMyClubsHandler class
    /// </summary>
    public GetMyClubsHandler(
        IClubManagerRepository clubManagerRepository,
        IClubRepository clubRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IHockeyTeamRepository hockeyTeamRepository,
        ILogger<GetMyClubsHandler> logger)
    {
        _clubManagerRepository = clubManagerRepository;
        _clubRepository = clubRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _hockeyTeamRepository = hockeyTeamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetMyClubsQuery request
    /// </summary>
    public async Task<Result<IEnumerable<ClubAdminClubDto>>> Handle(GetMyClubsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<ClubAdminClubDto> clubs = new();

            IEnumerable<ClubManager> managerRows = await _clubManagerRepository.GetAllByPersonIdAsync(request.PersonId);
            foreach (ClubManager managerRow in managerRows.Where(m => m.IsActive))
            {
                Club? club = await _clubRepository.GetByIdAsync(managerRow.ClubId);
                if (club == null)
                {
                    continue;
                }

                List<ClubAdminTeamDto> teams = new();

                IEnumerable<FloorballTeam?> floorballTeams = await _floorballTeamRepository.GetByClubIdAsync(club.Id);
                foreach (FloorballTeam? team in floorballTeams)
                {
                    if (team != null)
                    {
                        teams.Add(new ClubAdminTeamDto(
                            "floorball",
                            team.Id,
                            team.Name,
                            team.ShortName,
                            team.LogoUrl?.ToString()));
                    }
                }

                IEnumerable<FootballTeam?> footballTeams = await _footballTeamRepository.GetByClubIdAsync(club.Id);
                foreach (FootballTeam? team in footballTeams)
                {
                    if (team != null)
                    {
                        teams.Add(new ClubAdminTeamDto(
                            "football",
                            team.Id,
                            team.Name,
                            team.ShortName,
                            team.LogoUrl?.ToString()));
                    }
                }

                IReadOnlyList<HockeyTeam> hockeyTeams = await _hockeyTeamRepository.GetByClubIdAsync(club.Id);
                foreach (HockeyTeam team in hockeyTeams)
                {
                    teams.Add(new ClubAdminTeamDto(
                        "hockey",
                        team.Id,
                        team.Name,
                        team.ShortName,
                        team.LogoUrl?.ToString()));
                }

                clubs.Add(new ClubAdminClubDto(
                    club.Id,
                    club.Name,
                    club.City,
                    club.LogoUrl?.ToString(),
                    teams));
            }

            return Result<IEnumerable<ClubAdminClubDto>>.Success(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs for person {PersonId}", request.PersonId);
            return Result<IEnumerable<ClubAdminClubDto>>.Failure("An error occurred while retrieving the clubs.");
        }
    }
}
