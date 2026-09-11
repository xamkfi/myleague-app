using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for replacing the active field player lineup (and optional goalie) for a single team
/// in a match. Mirrors <see cref="ChangeGoalieHandler"/> but operates on the full lineup.
/// </summary>
public class SetMatchActiveRosterHandler : IRequestHandler<SetMatchActiveRosterCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<SetMatchActiveRosterHandler> _logger;

    public SetMatchActiveRosterHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<SetMatchActiveRosterHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(SetMatchActiveRosterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            // Load the target team via the team repository so its Roster collection is eagerly
            // included. EF's change tracker fixes up match.HomeTeam / match.AwayTeam to use the
            // same instance, which makes the roster validation inside the domain method work.
            // Without this, match.<Team>.Roster would be empty and every player would be
            // rejected with "is not on the team's roster".
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            _logger.LogInformation(
                "Setting active roster for match {MatchId}, team {TeamId}: {PlayerCount} players, goalie {GoalieId}",
                request.MatchId, request.TeamId, request.Players.Count, request.GoalieId);

            IEnumerable<ActivePlayerSelection> selections = request.Players
                .Select(p => new ActivePlayerSelection(p.PlayerId, p.Position));

            match.SetActiveRoster(request.TeamId, selections, request.GoalieId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation(
                "Successfully updated active roster for match {MatchId}, team {TeamId}",
                request.MatchId, request.TeamId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while setting active roster for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while setting active roster for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting active roster for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while updating the active roster.");
        }
    }
}
