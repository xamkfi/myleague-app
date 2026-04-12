using Application.Features.Floorball.Tournaments.Commands;
using Application.Common;
using Domain.Entities.Floorball;
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
/// Handler for deleting a floorball tournament
/// </summary>
public class DeleteFloorballTournamentHandler : IRequestHandler<DeleteFloorballTournamentCommand, Result>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballTournamentHandler> _logger;

    public DeleteFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball tournament with ID: {TournamentId}", request.CompetitionId);
                return Result.NotFound("FloorballTournament", request.CompetitionId);
            }

            IEnumerable<FloorballMatch> tournamentMatches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            bool hasMatches = tournamentMatches.Any();
            if (hasMatches)
            {
                _logger.LogWarning("Attempt to delete tournament with existing matches: {TournamentId}", request.CompetitionId);
                return Result.Failure("Cannot delete a tournament that has matches. Delete the matches first.");
            }

            _logger.LogInformation("Deleting floorball tournament with ID: {TournamentId}", request.CompetitionId);
            await _tournamentRepository.DeleteAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball tournament with ID: {TournamentId}", request.CompetitionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball tournament: {TournamentId}", request.CompetitionId);
            return Result.Failure("An error occurred while deleting the floorball tournament.");
        }
    }
}
