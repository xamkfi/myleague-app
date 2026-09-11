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
/// Handles removing staff from a hockey team.
/// </summary>
public class RemoveHockeyTeamStaffHandler : IRequestHandler<RemoveHockeyTeamStaffCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyTeamStaffHandler> _logger;

    public RemoveHockeyTeamStaffHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyTeamStaffHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(RemoveHockeyTeamStaffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.RemoveStaff(request.StaffId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed staff {StaffId} from hockey team {TeamId}", request.StaffId, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveHockeyTeamStaff for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveHockeyTeamStaff for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while removing staff from the team.", ex.Flatten());
        }
    }
}
