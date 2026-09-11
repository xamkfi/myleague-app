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

/// <summary>
/// Handler for ending a period in a floorball match
/// </summary>
public class EndPeriodHandler : IRequestHandler<EndPeriodCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<EndPeriodHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the EndPeriodHandler class
    /// </summary>
    public EndPeriodHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<EndPeriodHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the EndPeriodCommand request
    /// </summary>
    public async Task<Result<FloorballMatchDto>> Handle(EndPeriodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.EndPeriod(request.PeriodNumber);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while ending period {Period} for match {MatchId}", request.PeriodNumber, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while ending the period.");
        }
    }
}


