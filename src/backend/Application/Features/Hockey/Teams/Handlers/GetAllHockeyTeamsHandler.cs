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
/// Handles retrieving all hockey teams.
/// </summary>
public class GetAllHockeyTeamsHandler : IRequestHandler<GetAllHockeyTeamsQuery, Result<IEnumerable<HockeyTeamDto>>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllHockeyTeamsHandler> _logger;

    public GetAllHockeyTeamsHandler(
        IHockeyTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetAllHockeyTeamsHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyTeamDto>>> Handle(GetAllHockeyTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeam> teams = await _teamRepository.GetAllAsync();
            List<HockeyTeam> filtered = request.TeamCategory is null
                ? teams.ToList()
                : teams.Where(team => team.TeamCategory == request.TeamCategory).ToList();
            Dictionary<Guid, Club> clubs = await _clubRepository.GetByIdsAsync(
                filtered.Select(team => team.ClubId),
                cancellationToken);
            return Result<IEnumerable<HockeyTeamDto>>.Success(HockeyTeamMapper.ToDtos(filtered, clubs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all hockey teams");
            return Result<IEnumerable<HockeyTeamDto>>.Failure("An error occurred while retrieving hockey teams.", ex.Flatten());
        }
    }
}
