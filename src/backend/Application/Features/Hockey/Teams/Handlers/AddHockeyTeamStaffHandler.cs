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
/// Handles adding staff to a hockey team.
/// </summary>
public class AddHockeyTeamStaffHandler : IRequestHandler<AddHockeyTeamStaffCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyTeamStaffHandler> _logger;

    public AddHockeyTeamStaffHandler(
        IHockeyTeamRepository teamRepository,
        IPersonRepository personRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyTeamStaffHandler> logger)
    {
        _teamRepository = teamRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(AddHockeyTeamStaffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person is null)
            {
                return Result<HockeyTeamDto>.NotFound("Person", request.PersonId);
            }

            team.AddStaff(request.PersonId, request.Role, request.CompetitionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added staff person {PersonId} to hockey team {TeamId}", request.PersonId, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddHockeyTeamStaff for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddHockeyTeamStaff for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddHockeyTeamStaff for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while adding staff to the team.", ex.Flatten());
        }
    }
}
