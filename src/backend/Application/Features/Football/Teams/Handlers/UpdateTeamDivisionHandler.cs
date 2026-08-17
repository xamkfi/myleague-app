
using MediatR;
using Application.Features.Football.Teams.Commands;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Microsoft.Extensions.Logging;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;

namespace Application.Features.Football.Teams.Handlers
{
    /// <summary>
    /// Handler for updating a division of an existing team
    /// </summary>
    public class UpdateTeamDivisionHandler : IRequestHandler<UpdateTeamDivisionCommand, Result<FootballTeamDto>>
    {
        private readonly ILogger<UpdateTeamDivisionHandler> _logger;
        private readonly IFootballTeamRepository _footballTeamRepository;
        private readonly IDivisionRepository _divisionRepository;
        private readonly IFootballUnitOfWork _footballUnitOfWork;
        private readonly IClubRepository _clubRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateTeamDivisionHandler
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="footballTeamRepository"></param>
        /// <param name="divisionRepository"></param>
        /// <param name="footballUnitOfWork"></param>
        /// <param name="clubRepository"></param>
        public UpdateTeamDivisionHandler(ILogger<UpdateTeamDivisionHandler> logger, IFootballTeamRepository footballTeamRepository, IDivisionRepository divisionRepository, IFootballUnitOfWork footballUnitOfWork, IClubRepository clubRepository)
        {
            _logger = logger;
            _footballTeamRepository = footballTeamRepository;
            _divisionRepository = divisionRepository;
            _footballUnitOfWork = footballUnitOfWork;
            _clubRepository = clubRepository;
        }

        /// <summary>
        /// Handles the update team division handler
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<FootballTeamDto>> Handle(UpdateTeamDivisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Find existing team
                FootballTeam? team = await _footballTeamRepository.GetByIdAsync(request.teamId);
                if (team == null)
                {
                    return Result<FootballTeamDto>.NotFound("Team not found", request.teamId);
                }
                Division? division = await _divisionRepository.GetByIdAsync(request.divisionId);
                if (division == null)
                {
                    return Result<FootballTeamDto>.NotFound("Division not found", request.divisionId);
                }
                //update team division
                team.UpdateDivision(request.divisionId);

                //Get club for the DTO object
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club == null)
                {
                    return Result<FootballTeamDto>.NotFound("Club not found", team.ClubId);
                }
                //Save changes to database
                await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

                //Create the DTO
                FootballTeamDto teamDto = FootballTeamMapper.ToDto(team, club);

                return Result<FootballTeamDto>.Success(teamDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating division {divisionId} in team {TeamId}", request.divisionId, request.teamId);
                return Result<FootballTeamDto>.Failure("An error occurred while updating the division in the team.");
            }

        }
    }
}
