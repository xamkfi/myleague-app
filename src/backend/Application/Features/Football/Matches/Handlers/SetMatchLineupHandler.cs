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

public class SetMatchLineupHandler : IRequestHandler<SetMatchLineupCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<SetMatchLineupHandler> _logger;

    public SetMatchLineupHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<SetMatchLineupHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(SetMatchLineupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FootballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            IEnumerable<FootballLineupSelection> selections = request.Players
                .Select(p => new FootballLineupSelection(p.PlayerId, p.Position, p.IsOnField));

            match.SetLineup(request.TeamId, selections);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while setting lineup for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while setting lineup for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting lineup for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while updating the lineup.");
        }
    }
}
