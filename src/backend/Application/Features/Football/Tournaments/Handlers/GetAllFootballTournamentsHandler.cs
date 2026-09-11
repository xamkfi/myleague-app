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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for retrieving all football tournaments
/// </summary>
public class GetAllFootballTournamentsHandler : IRequestHandler<GetAllFootballTournamentsQuery, Result<List<FootballTournamentDto>>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetAllFootballTournamentsHandler> _logger;

    public GetAllFootballTournamentsHandler(
        IFootballTournamentRepository tournamentRepository,
        ILogger<GetAllFootballTournamentsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballTournamentDto>>> Handle(GetAllFootballTournamentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all football tournaments");

            IEnumerable<FootballTournament> tournaments = await _tournamentRepository.GetAllAsync(request.TeamCategory, cancellationToken);
            List<FootballTournamentDto> tournamentDtos = tournaments
                .Select(t => FootballTournamentMapper.ToDto(t))
                .ToList();

            _logger.LogInformation("Successfully retrieved {TournamentCount} football tournaments", tournamentDtos.Count);

            return Result<List<FootballTournamentDto>>.Success(tournamentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all football tournaments");
            return Result<List<FootballTournamentDto>>.Failure("An error occurred while retrieving football tournaments.");
        }
    }
}
