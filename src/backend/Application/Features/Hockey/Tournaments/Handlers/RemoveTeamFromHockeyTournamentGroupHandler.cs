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
/// Handles RemoveTeamFromHockeyTournamentGroup.
/// </summary>
public class RemoveTeamFromHockeyTournamentGroupHandler : IRequestHandler<RemoveTeamFromHockeyTournamentGroupCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromHockeyTournamentGroupHandler> _logger;

    public RemoveTeamFromHockeyTournamentGroupHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromHockeyTournamentGroupHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        RemoveTeamFromHockeyTournamentGroupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            tournament.RemoveTeamFromGroup(request.GroupId, request.CompetitionTeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("RemoveTeamFromHockeyTournamentGroup completed for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveTeamFromHockeyTournamentGroup for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RemoveTeamFromHockeyTournamentGroup for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveTeamFromHockeyTournamentGroup for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while removing the team from the tournament group.", ex.Flatten());
        }
    }
}
