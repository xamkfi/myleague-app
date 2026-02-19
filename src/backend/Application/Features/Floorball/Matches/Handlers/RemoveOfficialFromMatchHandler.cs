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

public class RemoveOfficialFromMatchHandler : IRequestHandler<RemoveOfficialFromMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveOfficialFromMatchHandler> _logger;

    public RemoveOfficialFromMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveOfficialFromMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RemoveOfficialFromMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.RemoveOfficial(request.RefereeId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException inv)
        {
            _logger.LogWarning(inv, "Validation error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(inv.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while removing the official.");
        }
    }
}

