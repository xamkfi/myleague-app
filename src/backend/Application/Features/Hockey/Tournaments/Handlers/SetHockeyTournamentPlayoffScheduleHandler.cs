using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Hockey.Matches;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles setting the playoff schedule on a hockey tournament.
/// </summary>
public class SetHockeyTournamentPlayoffScheduleHandler
    : IRequestHandler<SetHockeyTournamentPlayoffScheduleCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyTournamentPlayoffScheduleHandler> _logger;

    public SetHockeyTournamentPlayoffScheduleHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyTournamentPlayoffScheduleHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        SetHockeyTournamentPlayoffScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            List<HockeyPlayoffScheduleSlot> slots = request.Slots.Select(s => new HockeyPlayoffScheduleSlot(
                s.Round,
                s.SeriesOrder,
                s.MatchOrder,
                s.HomeSourceType,
                s.AwaySourceType,
                s.HomeSourceGroupId,
                s.AwaySourceGroupId,
                s.HomeSourceSeriesId,
                s.AwaySourceSeriesId,
                s.HomeSourceRank,
                s.AwaySourceRank,
                s.ManualHomeCompetitionTeamId,
                s.ManualAwayCompetitionTeamId)).ToList();

            tournament.SetPlayoffSchedule(slots);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Set playoff schedule on tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected playoff schedule for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid playoff schedule for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed playoff schedule for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while setting the playoff schedule.", ex.Flatten());
        }
    }
}
