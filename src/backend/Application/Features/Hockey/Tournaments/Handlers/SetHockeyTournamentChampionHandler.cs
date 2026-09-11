using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;


namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles SetHockeyTournamentChampion.
/// </summary>
public class SetHockeyTournamentChampionHandler : IRequestHandler<SetHockeyTournamentChampionCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyTournamentChampionHandler> _logger;

    public SetHockeyTournamentChampionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyTournamentChampionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        SetHockeyTournamentChampionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            tournament.SetChampion(request.ChampionCompetitionTeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SetHockeyTournamentChampion completed for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected SetHockeyTournamentChampion for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid SetHockeyTournamentChampion for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed SetHockeyTournamentChampion for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while setting the tournament champion.", ex.Flatten());
        }
    }
}
