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
/// Handles UpdateHockeyTeamLogo.
/// </summary>
public class UpdateHockeyTeamLogoHandler : IRequestHandler<UpdateHockeyTeamLogoCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyTeamLogoHandler> _logger;

    public UpdateHockeyTeamLogoHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyTeamLogoHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(UpdateHockeyTeamLogoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            Uri? logoUri = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : new Uri(request.LogoUrl);
            team.UpdateLogo(logoUri);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated logo on hockey team {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyTeamLogo for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyTeamLogo for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while updating the team logo.", ex.Flatten());
        }
    }
}
