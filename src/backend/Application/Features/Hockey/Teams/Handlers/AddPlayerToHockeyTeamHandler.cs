using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Handles adding a player to a hockey team roster.
/// </summary>
public class AddPlayerToHockeyTeamHandler : IRequestHandler<AddPlayerToHockeyTeamCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddPlayerToHockeyTeamHandler> _logger;

    public AddPlayerToHockeyTeamHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyPlayerRepository playerRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddPlayerToHockeyTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(AddPlayerToHockeyTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            HockeyPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyPlayer", request.PlayerId);
            }

            team.AddPlayer(
                player,
                request.Position,
                request.CompetitionId,
                request.JerseyNumber,
                request.RequestedJerseyNumber,
                request.RosterStatus);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added player {PlayerId} to hockey team {TeamId}", request.PlayerId, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddPlayerToHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddPlayerToHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddPlayerToHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while adding the player to the team.", ex.Flatten());
        }
    }
}
