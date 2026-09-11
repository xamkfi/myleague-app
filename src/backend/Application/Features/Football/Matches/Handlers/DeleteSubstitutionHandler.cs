using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class DeleteSubstitutionHandler : IRequestHandler<DeleteSubstitutionCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSubstitutionHandler> _logger;

    public DeleteSubstitutionHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteSubstitutionHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(DeleteSubstitutionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.DeleteSubstitutionEvent(request.SubstitutionEventId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting substitution {SubstitutionId} from match {MatchId}",
                request.SubstitutionEventId,
                request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
    }
}
