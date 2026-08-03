using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Handles creation of a new hockey team.
/// </summary>
public class CreateHockeyTeamHandler : IRequestHandler<CreateHockeyTeamCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyTeamHandler> _logger;

    public CreateHockeyTeamHandler(
        IHockeyTeamRepository teamRepository,
        IClubRepository clubRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(CreateHockeyTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club is null)
            {
                return Result<HockeyTeamDto>.Failure("Club not found.");
            }

            HockeyTeam team = new(
                request.Name,
                club,
                request.TeamCategory,
                request.DivisionId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.SecondaryJerseyColor,
                request.ShortName);

            await _teamRepository.AddAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created hockey team {TeamId} ({Name})", team.Id, team.Name);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create hockey team {Name}", request.Name);
            return Result<HockeyTeamDto>.Failure("An error occurred while creating the hockey team.");
        }
    }
}
