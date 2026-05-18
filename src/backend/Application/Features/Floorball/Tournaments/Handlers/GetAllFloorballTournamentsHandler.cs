using Application.Features.Floorball.Tournaments.Queries;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
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
/// Handler for retrieving all floorball tournaments
/// </summary>
public class GetAllFloorballTournamentsHandler : IRequestHandler<GetAllFloorballTournamentsQuery, Result<List<FloorballTournamentDto>>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetAllFloorballTournamentsHandler> _logger;

    public GetAllFloorballTournamentsHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetAllFloorballTournamentsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballTournamentDto>>> Handle(GetAllFloorballTournamentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all floorball tournaments");

            IEnumerable<FloorballTournament> tournaments = await _tournamentRepository.GetAllAsync();
            List<FloorballTournamentDto> tournamentDtos = tournaments
                .Select(t => FloorballTournamentMapper.ToDto(t))
                .ToList();

            _logger.LogInformation("Successfully retrieved {TournamentCount} floorball tournaments", tournamentDtos.Count);

            return Result<List<FloorballTournamentDto>>.Success(tournamentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all floorball tournaments");
            return Result<List<FloorballTournamentDto>>.Failure("An error occurred while retrieving floorball tournaments.");
        }
    }
}
