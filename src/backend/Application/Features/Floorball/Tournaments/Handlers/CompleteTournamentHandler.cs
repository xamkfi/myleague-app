using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for completing a tournament
/// </summary>
public class CompleteTournamentHandler : IRequestHandler<CompleteTournamentCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteTournamentHandler> _logger;

    public CompleteTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CompleteTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(CompleteTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.CompetitionId);
            }

            // Refuse to complete the tournament while any match is still pending. The domain entity
            // doesn't see its matches collection in the loaded state, so this guard lives here.
            IEnumerable<FloorballMatch> tournamentMatches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            int unfinishedCount = tournamentMatches.Count(m =>
                m.Status == FloorballMatchStatus.Scheduled ||
                m.Status == FloorballMatchStatus.InProgress ||
                m.Status == FloorballMatchStatus.Postponed);
            if (unfinishedCount > 0)
            {
                string message = $"Cannot complete tournament: {unfinishedCount} match(es) are still unfinished.";
                _logger.LogWarning("{Message} TournamentId={TournamentId}", message, request.CompetitionId);
                return Result<FloorballTournamentDto>.Failure(message);
            }

            _logger.LogInformation("Completing tournament: {TournamentId}", request.CompetitionId);
            tournament.CompleteTournament();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully completed tournament: {TournamentId}", request.CompetitionId);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while completing tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while completing the tournament.",
                ex.Flatten());
        }
    }
}
