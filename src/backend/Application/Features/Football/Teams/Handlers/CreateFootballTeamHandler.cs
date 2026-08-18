using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for creating a new football team
/// </summary>
public class CreateFootballTeamHandler : IRequestHandler<CreateFootballTeamCommand, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFootballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFootballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The football unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFootballTeamHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CreateFootballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFootballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created team as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamDto>> Handle(CreateFootballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                return Result<FootballTeamDto>.Failure("Club not found");
            }
            // Create the team entity
            FootballTeam team = FootballTeamMapper.ToEntity(request, club);

            _logger.LogInformation("Creating new football team: {TeamName}", request.Name);
            await _teamRepository.AddAsync(team);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTeamDto teamDto = FootballTeamMapper.ToDto(team, club);
            _logger.LogInformation("Successfully created football team with ID: {TeamId}", team.Id);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football team: {TeamName}", request.Name);
            return Result<FootballTeamDto>.Failure("An error occurred while creating the football team.");
        }
    }
} 
