using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles creating a hockey match.
/// </summary>
public class CreateHockeyMatchHandler : IRequestHandler<CreateHockeyMatchCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyMatchHandler> _logger;

    public CreateHockeyMatchHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(CreateHockeyMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = null;
            HockeyMatchRules? matchRules = null;
            bool usesLineManagement = false;

            if (request.CompetitionId is Guid competitionId)
            {
                competition = await _competitionRepository.GetByIdAsync(competitionId);
                if (competition is null)
                {
                    return Result<HockeyMatchDto>.NotFound("HockeyCompetition", competitionId);
                }

                if (competition.IsCompleted || competition.Status == Domain.Enums.Hockey.Competitions.HockeyCompetitionStatus.Cancelled)
                {
                    return Result<HockeyMatchDto>.Failure(
                        $"Cannot create a match for a competition in status {competition.Status}.");
                }

                HockeyCompetitionRules effective = competition.GetEffectiveRules();
                matchRules = effective.MatchRules;
                usesLineManagement = effective.RosterRules.LineManagementEnabled;
            }

            HockeyMatch match = new(
                DateTimeUtc.Normalize(request.ScheduledStartTime),
                request.MatchType,
                matchRules: matchRules,
                competitionId: request.CompetitionId,
                competitionDivisionId: request.CompetitionDivisionId,
                tournamentGroupId: request.TournamentGroupId,
                playoffSeriesId: request.PlayoffSeriesId,
                venue: request.Venue,
                usesLineManagement: usesLineManagement);

            if (request.PlayoffRound is HockeyPlayoffRound playoffRound)
            {
                match.SetPlayoffInfo(
                    playoffRound,
                    request.PlayoffMatchOrder ?? 0,
                    request.NextMatchId,
                    request.NextMatchSlot);
            }

            if (competition is not null)
            {
                competition.AddMatch(match);
            }
            else
            {
                await _matchRepository.AddAsync(match);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created hockey match {MatchId}", match.Id);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected CreateHockeyMatch");
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CreateHockeyMatch");
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed CreateHockeyMatch");
            return Result<HockeyMatchDto>.Failure("An error occurred while creating the hockey match.", ex.Flatten());
        }
    }
}
