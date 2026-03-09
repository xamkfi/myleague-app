using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Features.Floorball.Tournaments.Queries;
using Domain.Entities.Floorball.Tournament;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for retrieving a floorball tournament by its unique identifier
/// </summary>
public class GetFloorballTournamentByIdHandler
    : IRequestHandler<GetFloorballTournamentByIdQuery, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetFloorballTournamentByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTournamentByIdHandler class
    /// </summary>
    /// <param name="tournamentRepository">The floorball tournament repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTournamentByIdHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetFloorballTournamentByIdHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTournamentByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the tournament ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tournament as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballTournamentDto>> Handle(
        GetFloorballTournamentByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball tournament with ID: {TournamentId}", request.Id);

            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.Id);
            if (tournament == null)
            {
                _logger.LogWarning("Floorball tournament with ID {TournamentId} not found", request.Id);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.Id);
            }

            FloorballTournamentDto dto = FloorballTournamentMapper.ToDto(tournament);

            _logger.LogInformation("Successfully retrieved floorball tournament: {TournamentId}", tournament.Id);
            return Result<FloorballTournamentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball tournament: {TournamentId}", request.Id);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while retrieving the floorball tournament.");
        }
    }
}
