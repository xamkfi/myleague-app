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
/// Handler for cancelling a tournament
/// </summary>
public class CancelTournamentHandler : IRequestHandler<CancelTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CancelTournamentHandler> _logger;

    public CancelTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CancelTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(CancelTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            _logger.LogInformation("Cancelling tournament: {TournamentId}", request.CompetitionId);
            tournament.CancelTournament();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully cancelled tournament: {TournamentId}", request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while cancelling tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while cancelling the tournament.",
                ex.Flatten());
        }
    }
}
