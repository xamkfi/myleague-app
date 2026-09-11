using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handles <see cref="AssignMatchTeamsCommand"/> — sets or clears the home/away team slots on a
/// scheduled or postponed match, and propagates the change forward through the playoff bracket
/// when applicable.
/// </summary>
/// <remarks>
/// Propagation rules:
/// <list type="bullet">
///   <item>When the match has a <c>NextMatchId</c> + <c>NextMatchSlot</c>, the projected team in
///         the next match's matching slot is updated to mirror this match's new home team (the
///         higher seed in the cross-group seeding model, which is the same heuristic used by
///         <c>StartTournamentPlayoffStageHandler.FillProjectedTeams</c>).</item>
///   <item>Propagation stops as soon as it reaches a match that is no longer Scheduled/Postponed
///         (anything started or completed has its real winner already, so overwriting would be
///         destructive).</item>
///   <item>Propagation also stops when the next match has not been loaded into the repository
///         (defensive: the caller can run the command again after the missing match is created).</item>
/// </list>
/// </remarks>
public class AssignMatchTeamsHandler : IRequestHandler<AssignMatchTeamsCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AssignMatchTeamsHandler> _logger;

    public AssignMatchTeamsHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AssignMatchTeamsHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(AssignMatchTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("AssignMatchTeams: match {MatchId} not found.", request.MatchId);
                return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);
            }

            if (match.Status != FloorballMatchStatus.Scheduled && match.Status != FloorballMatchStatus.Postponed)
            {
                return Result<FloorballMatchDto>.Failure(
                    $"Joukkueita voi muuttaa vain ajastetuille (Scheduled tai Postponed) otteluille. Nykyinen tila: {match.Status}.");
            }

            // Resolve teams up-front so we can fail fast with a clear NotFound rather than letting
            // the domain entity store a half-applied state.
            FloorballTeam? homeTeam = null;
            if (request.HomeTeamId.HasValue)
            {
                homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId.Value);
                if (homeTeam == null)
                {
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.HomeTeamId.Value);
                }
            }

            FloorballTeam? awayTeam = null;
            if (request.AwayTeamId.HasValue)
            {
                awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId.Value);
                if (awayTeam == null)
                {
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.AwayTeamId.Value);
                }
            }

            // Capture the prior values so we know whether each slot actually changed; propagation
            // only fires when the slot value moved, otherwise we'd touch downstream matches for no
            // reason.
            Guid? previousHomeTeamId = match.HomeTeamId;
            Guid? previousAwayTeamId = match.AwayTeamId;

            match.AssignTeam(FloorballPlayoffSlot.Home, homeTeam);
            match.AssignTeam(FloorballPlayoffSlot.Away, awayTeam);

            // Playoff propagation: bump the projected team in the next bracket slot when our
            // higher seed (= home team, per the seeding convention) has changed. Loser-slot
            // propagation (semifinal losers → third-place match) is handled at match completion
            // time, not here, so we only follow the winner-forward chain.
            if (match.NextMatchId.HasValue
                && match.NextMatchSlot.HasValue
                && previousHomeTeamId != match.HomeTeamId)
            {
                await PropagateProjectedHomeTeamAsync(match, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Assigned teams on match {MatchId}: home={HomeTeamId}, away={AwayTeamId}.",
                match.Id, match.HomeTeamId, match.AwayTeamId);

            FloorballMatchDto dto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rule rejected AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while updating match teams.");
        }
    }

    /// <summary>
    /// Walks the <c>NextMatchId</c> chain forward and overwrites the projected team in each
    /// downstream slot that still uses the previous projection. Stops at the first match that is
    /// not Scheduled/Postponed (it has already started or finished and owns its real participant)
    /// or that cannot be loaded from the repository.
    /// </summary>
    private async Task PropagateProjectedHomeTeamAsync(FloorballMatch source, CancellationToken cancellationToken)
    {
        FloorballMatch? cursor = source;

        while (cursor != null
            && cursor.NextMatchId.HasValue
            && cursor.NextMatchSlot.HasValue)
        {
            FloorballMatch? nextMatch = await _matchRepository.GetByIdAsync(cursor.NextMatchId.Value);
            if (nextMatch == null)
            {
                _logger.LogWarning(
                    "Skipping playoff propagation from match {SourceId}: next match {NextMatchId} not found.",
                    cursor.Id, cursor.NextMatchId.Value);
                return;
            }

            if (nextMatch.Status != FloorballMatchStatus.Scheduled
                && nextMatch.Status != FloorballMatchStatus.Postponed)
            {
                // Downstream match already started — leave its actual participant alone.
                return;
            }

            // The seeding convention projects the higher seed (= the source match's home team)
            // into the next match. Mirror that here, which also handles the "clear team" case by
            // passing the (possibly null) HomeTeam through.
            nextMatch.AssignTeam(cursor.NextMatchSlot.Value, cursor.HomeTeam);

            cursor = nextMatch;
        }
    }
}
