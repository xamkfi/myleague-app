using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for starting the group stage of a tournament
/// </summary>
public class StartTournamentGroupStageHandler : IRequestHandler<StartTournamentGroupStageCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<StartTournamentGroupStageHandler> _logger;

    public StartTournamentGroupStageHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<StartTournamentGroupStageHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(StartTournamentGroupStageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            _logger.LogInformation("Starting group stage for tournament: {TournamentId}", request.CompetitionId);
            tournament.StartGroupStage();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully started group stage for tournament: {TournamentId}", request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while starting group stage for tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting group stage for tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while starting the tournament group stage.",
                ex.Flatten());
        }
    }
}
