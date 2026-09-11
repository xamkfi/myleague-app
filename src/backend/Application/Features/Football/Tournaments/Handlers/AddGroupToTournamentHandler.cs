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
/// Handler for adding a group to a tournament
/// </summary>
public class AddGroupToTournamentHandler : IRequestHandler<AddGroupToTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddGroupToTournamentHandler> _logger;

    public AddGroupToTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddGroupToTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(AddGroupToTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Load the tournament without change tracking. The parent aggregate participates only
            // in domain validation and order computation; we deliberately avoid putting it (and its
            // owned types) into the change tracker because EF Core 9's TPH + owned-type change
            // detection has been observed to mark the parent as Modified spuriously, leading to a
            // DbUpdateConcurrencyException ("expected 1 row, actually 0") on SaveChanges.
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            _logger.LogInformation("Adding group '{GroupName}' to tournament: {TournamentId}", request.GroupName, request.CompetitionId);

            // AddGroup runs the domain rules (status guard, name validation) and computes the order.
            // The returned group is the only entity we need to persist — adding it directly via the
            // repository keeps the change tracker focused on a single INSERT.
            FootballTournamentGroup newGroup = tournament.AddGroup(request.GroupName);

            await _tournamentRepository.AddGroupAsync(newGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournament? refreshed = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(refreshed ?? tournament);
            _logger.LogInformation("Successfully added group '{GroupName}' to tournament: {TournamentId}", request.GroupName, request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while adding group to tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding group to tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while adding a group to the tournament.",
                ex.Flatten());
        }
    }
}
