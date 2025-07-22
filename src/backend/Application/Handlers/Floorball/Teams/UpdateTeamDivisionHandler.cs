
using MediatR;
using Application.Commands.Floorball.Team;
using Application.Common;
using Application.DTOs.Floorball;
using Microsoft.Extensions.Logging;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Application.Mappings.Floorball;

namespace Application.Handlers.Floorball.Teams
{
    /// <summary>
    /// Handler for updating a division of an existing team
    /// </summary>
    public class UpdateTeamDivisionHandler : IRequestHandler<UpdateTeamDivisionCommand, Result<FloorballTeamDto>>
    {
        private readonly ILogger<UpdateTeamDivisionHandler> _logger;
        private readonly IFloorballTeamRepository _floorballTeamRepository;
        private readonly IDivisionRepository _divisionRepository;
        private readonly IFloorballUnitOfWork _floorballUnitOfWork;
        private readonly IClubRepository _clubRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateTeamDivisionHandler
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="floorballTeamRepository"></param>
        /// <param name="divisionRepository"></param>
        /// <param name="floorballUnitOfWork"></param>
        /// <param name="clubRepository"></param>
        public UpdateTeamDivisionHandler(ILogger<UpdateTeamDivisionHandler> logger, IFloorballTeamRepository floorballTeamRepository, IDivisionRepository divisionRepository, IFloorballUnitOfWork floorballUnitOfWork, IClubRepository clubRepository)
        {
            _logger = logger;
            _floorballTeamRepository = floorballTeamRepository;
            _divisionRepository = divisionRepository;
            _floorballUnitOfWork = floorballUnitOfWork;
            _clubRepository = clubRepository;
        }

        /// <summary>
        /// Handles the update team division handler
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<FloorballTeamDto>> Handle(UpdateTeamDivisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Find existing team
                FloorballTeam? team = await _floorballTeamRepository.GetByIdAsync(request.teamId);
                if (team == null)
                {
                    return Result<FloorballTeamDto>.NotFound("Team not found", request.teamId);
                }
                Division? division = await _divisionRepository.GetByIdAsync(request.divisionId);
                if (division == null)
                {
                    return Result<FloorballTeamDto>.NotFound("Division not found", request.divisionId);
                }
                //update team division
                team.UpdateDivision(request.divisionId);

                //Get club for the DTO object
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club == null)
                {
                    return Result<FloorballTeamDto>.NotFound("Club not found", team.ClubId);
                }
                //Save changes to database
                await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

                //Create the DTO
                FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team, club);

                return Result<FloorballTeamDto>.Success(teamDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating division {divisionId} in team {TeamId}", request.divisionId, request.teamId);
                return Result<FloorballTeamDto>.Failure("An error occurred while updating the division in the team.");
            }

        }
    }
}
