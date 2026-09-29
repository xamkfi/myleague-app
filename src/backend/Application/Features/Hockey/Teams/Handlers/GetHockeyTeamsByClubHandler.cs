using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Application.Features.Hockey.Teams.Queries;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Handles retrieving hockey teams by club.
/// </summary>
public class GetHockeyTeamsByClubHandler : IRequestHandler<GetHockeyTeamsByClubQuery, Result<IEnumerable<HockeyTeamDto>>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetHockeyTeamsByClubHandler> _logger;

    public GetHockeyTeamsByClubHandler(
        IHockeyTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetHockeyTeamsByClubHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyTeamDto>>> Handle(GetHockeyTeamsByClubQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeam> teams = await _teamRepository.GetByClubIdAsync(request.ClubId);
            List<HockeyTeam> filtered = request.TeamCategory is null
                ? teams.ToList()
                : teams.Where(team => team.TeamCategory == request.TeamCategory).ToList();
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            Dictionary<Guid, Club> clubs = club is null
                ? new Dictionary<Guid, Club>()
                : new Dictionary<Guid, Club> { [club.Id] = club };
            return Result<IEnumerable<HockeyTeamDto>>.Success(HockeyTeamMapper.ToDtos(filtered, clubs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey teams for club {ClubId}", request.ClubId);
            return Result<IEnumerable<HockeyTeamDto>>.Failure("An error occurred while retrieving hockey teams.", ex.Flatten());
        }
    }
}
