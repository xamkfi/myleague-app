using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles assigning home and away teams to a hockey match.
/// </summary>
public class AddHomeAwayTeamsToHockeyMatchHandler
    : IRequestHandler<AddHomeAwayTeamsToHockeyMatchCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHomeAwayTeamsToHockeyMatchHandler> _logger;
    private readonly HockeyMatchValidationService _validationService = new();

    public AddHomeAwayTeamsToHockeyMatchHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHomeAwayTeamsToHockeyMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        AddHomeAwayTeamsToHockeyMatchCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.HomeTeamId == request.AwayTeamId)
            {
                return Result<HockeyMatchDto>.Failure("Home and away teams must be different.");
            }

            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            HockeyTeam? homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId);
            if (homeTeam is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyTeam", request.HomeTeamId);
            }

            HockeyTeam? awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId);
            if (awayTeam is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyTeam", request.AwayTeamId);
            }

            HockeyCompetitionTeam? homeCompetitionTeam = null;
            HockeyCompetitionTeam? awayCompetitionTeam = null;
            if (match.CompetitionId is Guid competitionId)
            {
                HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(competitionId);
                if (competition is null)
                {
                    return Result<HockeyMatchDto>.NotFound("HockeyCompetition", competitionId);
                }

                homeCompetitionTeam = competition.GetCompetitionTeam(request.HomeTeamId);
                if (homeCompetitionTeam is null || !homeCompetitionTeam.IsActive)
                {
                    return Result<HockeyMatchDto>.Failure("Home team is not an active member of the competition.");
                }

                awayCompetitionTeam = competition.GetCompetitionTeam(request.AwayTeamId);
                if (awayCompetitionTeam is null || !awayCompetitionTeam.IsActive)
                {
                    return Result<HockeyMatchDto>.Failure("Away team is not an active member of the competition.");
                }
            }

            match.AssignMatchTeam(request.HomeTeamId, HockeyTeamSlot.Home, homeCompetitionTeam);
            match.AssignMatchTeam(request.AwayTeamId, HockeyTeamSlot.Away, awayCompetitionTeam);

            HockeyDomainValidationResult validation = _validationService.ValidateHomeAway(match);
            if (!validation.IsValid)
            {
                return Result<HockeyMatchDto>.Failure(string.Join(" ", validation.Errors), validation.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Assigned home {HomeTeamId} and away {AwayTeamId} on match {MatchId}",
                request.HomeTeamId,
                request.AwayTeamId,
                request.MatchId);

            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddHomeAwayTeams for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddHomeAwayTeams for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddHomeAwayTeams for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(
                "An error occurred while assigning teams to the hockey match.",
                ex.Flatten());
        }
    }
}
