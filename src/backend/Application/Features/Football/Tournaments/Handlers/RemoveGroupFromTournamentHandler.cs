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
/// Handler for removing a group from a tournament
/// </summary>
public class RemoveGroupFromTournamentHandler : IRequestHandler<RemoveGroupFromTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveGroupFromTournamentHandler> _logger;

    public RemoveGroupFromTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RemoveGroupFromTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(RemoveGroupFromTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            FootballTournamentGroup? group = tournament.GetGroup(request.GroupId);
            if (group == null)
            {
                _logger.LogWarning("Group not found with ID: {GroupId} in tournament: {TournamentId}", request.GroupId, request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournamentGroup", request.GroupId);
            }

            _logger.LogInformation("Removing group {GroupId} from tournament: {TournamentId}", request.GroupId, request.CompetitionId);
            tournament.RemoveGroup(request.GroupId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully removed group {GroupId} from tournament: {TournamentId}", request.GroupId, request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while removing group from tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing group {GroupId} from tournament: {TournamentId}", request.GroupId, request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while removing a group from the tournament.",
                ex.Flatten());
        }
    }
}
