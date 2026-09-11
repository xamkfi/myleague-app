using Application.Features.Floorball.Matches.Commands;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

public class UpdateMatchOfficialsHandler : IRequestHandler<UpdateMatchOfficialsCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMatchOfficialsHandler> _logger;

    public UpdateMatchOfficialsHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballRefereeRepository refereeRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateMatchOfficialsHandler> logger)
    {
        _matchRepository = matchRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(UpdateMatchOfficialsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            if (request.OfficialIds == null || request.OfficialIds.Count == 0)
            {
                return Result<FloorballMatchDto>.Failure("Match must have at least one official.");
            }

            var referees = new List<FloorballReferee>();
            foreach (Guid refereeId in request.OfficialIds.Distinct())
            {
                FloorballReferee? referee = await _refereeRepository.GetByIdAsync(refereeId);
                if (referee == null)
                {
                    _logger.LogWarning("Referee not found with ID: {RefereeId}", refereeId);
                    return Result<FloorballMatchDto>.Failure($"Referee with ID {refereeId} not found.");
                }
                referees.Add(referee);
            }

            match.SetOfficials(referees);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating officials for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while updating match officials.");
        }
    }
}

