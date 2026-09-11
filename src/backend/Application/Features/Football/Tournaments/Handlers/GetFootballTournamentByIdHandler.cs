using Application.Features.Football.Tournaments.Queries;
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
/// Handler for retrieving a football tournament by ID
/// </summary>
public class GetFootballTournamentByIdHandler : IRequestHandler<GetFootballTournamentByIdQuery, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetFootballTournamentByIdHandler> _logger;

    public GetFootballTournamentByIdHandler(
        IFootballTournamentRepository tournamentRepository,
        ILogger<GetFootballTournamentByIdHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(GetFootballTournamentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football tournament with ID: {TournamentId}", request.CompetitionId);

            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Football tournament with ID {TournamentId} not found", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully retrieved football tournament: {TournamentId}", tournament.Id);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure("An error occurred while retrieving the football tournament.");
        }
    }
}
