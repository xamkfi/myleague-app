using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for <see cref="AddOfficialToMatchCommand"/>. Loads the target match and the
/// referee in one transactional unit and appends the referee through the domain entity's
/// invariants. The previous WebAPI controller used to do this with a Get + Update round-trip
/// from the controller body — this handler centralises the logic so the controller stays
/// thin and validation happens in one place.
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

    public async Task<Result<FloorballMatchDto>> Handle(AddOfficialToMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FloorballReferee? referee = await _refereeRepository.GetByIdAsync(request.RefereeId);
            if (referee is null)
            {
                _logger.LogWarning("Referee not found with ID: {RefereeId}", request.RefereeId);
                return Result<FloorballMatchDto>.Failure($"Referee with ID {request.RefereeId} not found.");
            }

            match.AddOfficial(referee);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException inv)
        {
            _logger.LogWarning(inv, "Validation error adding official {RefereeId} to match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(inv.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding official {RefereeId} to match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while adding the official.");
        }
    }
}
