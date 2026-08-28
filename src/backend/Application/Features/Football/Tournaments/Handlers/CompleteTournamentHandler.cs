using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for completing a tournament
/// </summary>
public class CompleteTournamentHandler : IRequestHandler<CompleteTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteTournamentHandler> _logger;

    public CompleteTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CompleteTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(CompleteTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            // Refuse to complete the tournament while any match is still pending. The domain entity
            // doesn't see its matches collection in the loaded state, so this guard lives here.
            IEnumerable<FootballMatch> tournamentMatches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            int unfinishedCount = tournamentMatches.Count(m =>
                m.Status == FootballMatchStatus.Scheduled ||
                m.Status == FootballMatchStatus.InProgress ||
                m.Status == FootballMatchStatus.Postponed);
            if (unfinishedCount > 0)
            {
                string message = $"Cannot complete tournament: {unfinishedCount} match(es) are still unfinished.";
                _logger.LogWarning("{Message} TournamentId={TournamentId}", message, request.CompetitionId);
                return Result<FootballTournamentDto>.Failure(message);
            }

            _logger.LogInformation("Completing tournament: {TournamentId}", request.CompetitionId);
            tournament.CompleteTournament();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully completed tournament: {TournamentId}", request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while completing tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while completing the tournament.",
                ex.Flatten());
        }
    }
}
