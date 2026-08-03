using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Application.Features.Hockey.Teams.Queries;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

public class GetHockeyTeamByIdHandler : IRequestHandler<GetHockeyTeamByIdQuery, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly ILogger<GetHockeyTeamByIdHandler> _logger;

    public GetHockeyTeamByIdHandler(
        IHockeyTeamRepository teamRepository,
        ILogger<GetHockeyTeamByIdHandler> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(GetHockeyTeamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.Id);
            if (team is null)
            {
                return Result<HockeyTeamDto>.Failure("Hockey team not found.");
            }

            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey team {TeamId}", request.Id);
            return Result<HockeyTeamDto>.Failure("An error occurred while retrieving the hockey team.");
        }
    }
}
