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
/// Handles retrieving a hockey team by id.
/// </summary>
public class GetHockeyTeamByIdHandler : IRequestHandler<GetHockeyTeamByIdQuery, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetHockeyTeamByIdHandler> _logger;

    public GetHockeyTeamByIdHandler(
        IHockeyTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetHockeyTeamByIdHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(GetHockeyTeamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.Id);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.Id);
            }

            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team, request.CompetitionId, club?.LogoUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey team {TeamId}", request.Id);
            return Result<HockeyTeamDto>.Failure("An error occurred while retrieving the hockey team.", ex.Flatten());
        }
    }
}
