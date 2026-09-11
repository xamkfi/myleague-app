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
/// Handles adding a line to a hockey team.
/// </summary>
public class AddHockeyLineHandler : IRequestHandler<AddHockeyLineCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyLineHandler> _logger;

    public AddHockeyLineHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyLineHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(AddHockeyLineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.AddLine(request.Name, request.LineNumber, request.LineType, request.CompetitionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added line to hockey team {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while adding the line.", ex.Flatten());
        }
    }
}
