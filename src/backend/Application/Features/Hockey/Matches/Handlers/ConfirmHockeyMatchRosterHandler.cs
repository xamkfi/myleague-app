using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles setting and confirming a match-side roster.
/// </summary>
public class ConfirmHockeyMatchRosterHandler
    : IRequestHandler<ConfirmHockeyMatchRosterCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmHockeyMatchRosterHandler> _logger;

    public ConfirmHockeyMatchRosterHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ConfirmHockeyMatchRosterHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
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

            HockeyMatchPlayerSelection selection = matchTeam.CreateOrReplacePlayerSelection(
                request.Source,
                request.ConfirmedByUserId);

            foreach (Guid teamPlayerId in request.TeamPlayerIds.Distinct())
            {
                HockeyTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p => p.Id == teamPlayerId);
                if (teamPlayer is null)
                {
                    return Result<HockeyMatchDto>.NotFound("HockeyTeamPlayer", teamPlayerId);
                }

                bool isGoalie = teamPlayer.Position == HockeyPosition.Goalie;
                selection.AddActivePlayer(teamPlayer, isGoalie: isGoalie);
            }

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

            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed ConfirmRoster for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(
                "An error occurred while confirming the hockey match roster.",
                ex.Flatten());
        }
    }
}
