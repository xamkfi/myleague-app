using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles setting and confirming a match-side roster. A lineup corrected after the match
/// has finished triggers a statistics recalculation so games played and goalie records follow it.
/// </summary>
public class ConfirmHockeyMatchRosterHandler
    : IRequestHandler<ConfirmHockeyMatchRosterCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ConfirmHockeyMatchRosterHandler> _logger;

    public ConfirmHockeyMatchRosterHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ConfirmHockeyMatchRosterHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        ConfirmHockeyMatchRosterCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyMatchTeam? matchTeam = match.MatchTeams.FirstOrDefault(t => t.Id == request.MatchTeamId);
            if (matchTeam is null)
            {
                return Result<HockeyMatchDto>.Failure("Match team is not part of this match.");
            }

            HockeyTeam? team = await _teamRepository.GetByIdAsync(matchTeam.TeamId);
            if (team is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyTeam", matchTeam.TeamId);
            }

            HockeyRosterRules rosterRules = HockeyRosterRules.Default();
            if (match.CompetitionId is Guid competitionId)
            {
                HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(competitionId);
                if (competition is null)
                {
                    return Result<HockeyMatchDto>.NotFound("HockeyCompetition", competitionId);
                }

                rosterRules = competition.GetEffectiveRules().RosterRules;
            }

            List<HockeyTeamPlayer> teamPlayers = new();
            foreach (Guid teamPlayerId in request.TeamPlayerIds.Distinct())
            {
                HockeyTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p => p.Id == teamPlayerId);
                if (teamPlayer is null)
                {
                    return Result<HockeyMatchDto>.NotFound("HockeyTeamPlayer", teamPlayerId);
                }

                teamPlayers.Add(teamPlayer);
            }

            // Update the lineup in place so events recorded for kept players stay valid.
            HockeyMatchPlayerSelection selection = match.SyncPlayerSelection(
                matchTeam.Id,
                teamPlayers,
                request.Source,
                request.ConfirmedByUserId);

            HockeyDomainValidationResult validation = HockeyRosterValidationService.ValidateMatchSelection(selection, rosterRules);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(
                    string.Join(" ", validation.Errors),
                    validation.Errors);
            }

            selection.Confirm(request.ConfirmedByUserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Confirmed roster for match team {MatchTeamId} on match {MatchId}",
                request.MatchTeamId,
                request.MatchId);

            Result<HockeyMatchDto> result = Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
            await HockeyMatchHandlerSupport.RecalculateStatisticsIfFinishedAsync(_mediator, _logger, result, cancellationToken);
            return result;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected ConfirmRoster for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid ConfirmRoster for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
    }
}
