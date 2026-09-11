using Application.Features.Floorball.Tournaments.Queries;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for retrieving a floorball tournament by ID
/// </summary>
public class GetFloorballTournamentByIdHandler : IRequestHandler<GetFloorballTournamentByIdQuery, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetFloorballTournamentByIdHandler> _logger;

    public GetFloorballTournamentByIdHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetFloorballTournamentByIdHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(GetFloorballTournamentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball tournament with ID: {TournamentId}", request.CompetitionId);

            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Floorball tournament with ID {TournamentId} not found", request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.CompetitionId);
            }

            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully retrieved floorball tournament: {TournamentId}", tournament.Id);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure("An error occurred while retrieving the floorball tournament.");
        }
    }
}
