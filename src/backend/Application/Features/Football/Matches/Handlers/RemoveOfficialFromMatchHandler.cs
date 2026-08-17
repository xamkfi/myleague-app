using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class RemoveOfficialFromMatchHandler : IRequestHandler<RemoveOfficialFromMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveOfficialFromMatchHandler> _logger;

    public RemoveOfficialFromMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RemoveOfficialFromMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(RemoveOfficialFromMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.RemoveOfficial(request.RefereeId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException inv)
        {
            _logger.LogWarning(inv, "Validation error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FootballMatchDto>.Failure(inv.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while removing the official.");
        }
    }
}
