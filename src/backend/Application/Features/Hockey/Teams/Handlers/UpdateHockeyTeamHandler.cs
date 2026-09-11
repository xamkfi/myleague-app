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
/// Handles UpdateHockeyTeam.
/// </summary>
public class UpdateHockeyTeamHandler : IRequestHandler<UpdateHockeyTeamCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyTeamHandler> _logger;

    public UpdateHockeyTeamHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(UpdateHockeyTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.UpdateName(request.Name);
            team.UpdateShortName(request.ShortName);
            team.UpdateTeamCategory(request.TeamCategory);
            team.UpdateDivision(request.DivisionId);
            team.UpdateHomeArena(request.HomeArena);
            team.UpdateJerseyColors(request.PrimaryJerseyColor, request.SecondaryJerseyColor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated hockey team {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while updating the hockey team.", ex.Flatten());
        }
    }
}
