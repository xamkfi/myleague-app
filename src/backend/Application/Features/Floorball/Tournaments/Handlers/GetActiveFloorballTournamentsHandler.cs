using Application.Features.Floorball.Tournaments.Queries;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
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
/// Handler for retrieving active floorball tournaments
/// </summary>
public class GetActiveFloorballTournamentsHandler : IRequestHandler<GetActiveFloorballTournamentsQuery, Result<List<FloorballTournamentDto>>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetActiveFloorballTournamentsHandler> _logger;

    public GetActiveFloorballTournamentsHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetActiveFloorballTournamentsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballTournamentDto>>> Handle(GetActiveFloorballTournamentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving active floorball tournaments");

            IEnumerable<FloorballTournament> tournaments = await _tournamentRepository.GetActiveAsync(request.TeamCategory, cancellationToken);
            List<FloorballTournamentDto> tournamentDtos = tournaments
                .Select(t => FloorballTournamentMapper.ToDto(t))
                .ToList();

            _logger.LogInformation("Successfully retrieved {TournamentCount} active floorball tournaments", tournamentDtos.Count);

            return Result<List<FloorballTournamentDto>>.Success(tournamentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active floorball tournaments");
            return Result<List<FloorballTournamentDto>>.Failure("An error occurred while retrieving active floorball tournaments.");
        }
    }
}
