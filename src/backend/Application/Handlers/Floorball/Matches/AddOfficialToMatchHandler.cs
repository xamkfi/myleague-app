using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for adding an official (referee) to a floorball match
/// </summary>
public class AddOfficialToMatchHandler : IRequestHandler<AddOfficialToMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddOfficialToMatchHandler> _logger;

    public AddOfficialToMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballRefereeRepository refereeRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddOfficialToMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddOfficialToMatchCommand request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<FloorballMatchDto>> Handle(AddOfficialToMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FloorballReferee? referee = await _refereeRepository.GetByIdAsync(request.RefereeId);
            if (referee == null)
            {
                _logger.LogWarning("Referee not found with ID: {RefereeId}", request.RefereeId);
                return Result<FloorballMatchDto>.Failure($"Referee with ID {request.RefereeId} not found.");
            }

            match.AddOfficial(referee);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding official {RefereeId} to match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while adding the official.");
        }
    }
}


