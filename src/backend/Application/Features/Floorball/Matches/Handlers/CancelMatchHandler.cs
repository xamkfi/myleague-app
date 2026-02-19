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
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using Domain.Entities.Floorball;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// CancelMatchHandler is responsible for handling the cancellation of a floorball match.
/// </summary>
public class CancelMatchHandler : IRequestHandler<CancelMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CancelMatchHandler> _logger;

    public CancelMatchHandler(IFloorballMatchRepository matchRepository, IFloorballUnitOfWork unitOfWork, ILogger<CancelMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(CancelMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.Cancel();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while canceling match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while canceling the match.");
        }
    }
}


