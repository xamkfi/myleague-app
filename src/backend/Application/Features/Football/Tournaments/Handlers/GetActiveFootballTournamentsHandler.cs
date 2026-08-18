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
/// Handler for retrieving active football tournaments
/// </summary>
public class GetActiveFootballTournamentsHandler : IRequestHandler<GetActiveFootballTournamentsQuery, Result<List<FootballTournamentDto>>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetActiveFootballTournamentsHandler> _logger;

    public GetActiveFootballTournamentsHandler(
        IFootballTournamentRepository tournamentRepository,
        ILogger<GetActiveFootballTournamentsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballTournamentDto>>> Handle(GetActiveFootballTournamentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving active football tournaments");

            IEnumerable<FootballTournament> tournaments = await _tournamentRepository.GetActiveAsync(request.TeamCategory, cancellationToken);
            List<FootballTournamentDto> tournamentDtos = tournaments
                .Select(t => FootballTournamentMapper.ToDto(t))
                .ToList();

            _logger.LogInformation("Successfully retrieved {TournamentCount} active football tournaments", tournamentDtos.Count);

            return Result<List<FootballTournamentDto>>.Success(tournamentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active football tournaments");
            return Result<List<FootballTournamentDto>>.Failure("An error occurred while retrieving active football tournaments.");
        }
    }
}
