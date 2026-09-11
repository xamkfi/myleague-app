using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles updating hockey tournament rules.
/// </summary>
public class UpdateHockeyTournamentRulesHandler
    : IRequestHandler<UpdateHockeyTournamentRulesCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyTournamentRulesHandler> _logger;

    public UpdateHockeyTournamentRulesHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyTournamentRulesHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        UpdateHockeyTournamentRulesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            HockeyTournamentRules rules = new(
                request.Format,
                request.HasGroupStage,
                request.HasPlayoffs,
                request.HasBronzeGame,
                request.HasPlacementGames,
                request.TeamsAdvancingPerGroup,
                HockeyStandingRules.Default());

            tournament.UpdateTournamentRules(rules);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated rules for hockey tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected rules update for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid rules update for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rules for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while updating tournament rules.", ex.Flatten());
        }
    }
}
