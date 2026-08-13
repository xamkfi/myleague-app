using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class UpdateMatchOfficialsHandler : IRequestHandler<UpdateMatchOfficialsCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMatchOfficialsHandler> _logger;

    public UpdateMatchOfficialsHandler(
        IFootballMatchRepository matchRepository,
        IFootballRefereeRepository refereeRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateMatchOfficialsHandler> logger)
    {
        _matchRepository = matchRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(UpdateMatchOfficialsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            if (match.MatchRules.RequireOfficialsToStart && (request.OfficialIds == null || request.OfficialIds.Count == 0))
            {
                return Result<FootballMatchDto>.Failure("Match must have at least one official.");
            }

            List<FootballReferee> referees = new();
            foreach (Guid refereeId in (request.OfficialIds ?? Array.Empty<Guid>()).Distinct())
            {
                FootballReferee? referee = await _refereeRepository.GetByIdAsync(refereeId);
                if (referee == null)
                {
                    return Result<FootballMatchDto>.Failure($"Referee with ID {refereeId} not found.");
                }

                referees.Add(referee);
            }

            match.SetOfficials(referees);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating officials for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while updating match officials.");
        }
    }
}
