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

public class AddOfficialToMatchHandler : IRequestHandler<AddOfficialToMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddOfficialToMatchHandler> _logger;

    public AddOfficialToMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballRefereeRepository refereeRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddOfficialToMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(AddOfficialToMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FootballReferee? referee = await _refereeRepository.GetByIdAsync(request.RefereeId);
            if (referee is null)
            {
                return Result<FootballMatchDto>.Failure($"Referee with ID {request.RefereeId} not found.");
            }

            match.AddOfficial(referee);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException inv)
        {
            _logger.LogWarning(inv, "Validation error adding official {RefereeId} to match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FootballMatchDto>.Failure(inv.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding official {RefereeId} to match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while adding the official.");
        }
    }
}
