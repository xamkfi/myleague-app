
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
    public class UpdateTeamDivisionHandler : IRequestHandler<UpdateTeamDivisionCommand, Result<FloorballTeamDto>>
    {
        private readonly ILogger<UpdateTeamDivisionHandler> _logger;
        private readonly IFloorballTeamRepository _floorballTeamRepository;
        private readonly IDivisionRepository _divisionRepository;
        private readonly IFloorballUnitOfWork _floorballUnitOfWork;

        public UpdateTeamDivisionHandler(ILogger<UpdateTeamDivisionHandler> logger, IFloorballTeamRepository floorballTeamRepository, IDivisionRepository divisionRepository, IFloorballUnitOfWork floorballUnitOfWork)
        {
            _logger = logger;
            _floorballTeamRepository = floorballTeamRepository;
            _divisionRepository = divisionRepository;
            _floorballUnitOfWork = floorballUnitOfWork;
        }

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

                //Save changes to database
                await _floorballUnitOfWork.SaveChangesAsync();

                FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team);

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
