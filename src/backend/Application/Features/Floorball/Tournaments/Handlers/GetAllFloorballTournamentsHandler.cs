using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Features.Floorball.Tournaments.Queries;
using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for retrieving all floorball tournaments with optional status filtering
/// </summary>
public class GetAllFloorballTournamentsHandler
    : IRequestHandler<GetAllFloorballTournamentsQuery, Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetAllFloorballTournamentsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballTournamentsHandler class
    /// </summary>
    /// <param name="tournamentRepository">The floorball tournament repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballTournamentsHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetAllFloorballTournamentsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFloorballTournamentsQuery request
    /// </summary>
    /// <param name="request">The query containing optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of tournament summary DTOs wrapped in a Result</returns>
    public async Task<Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>> Handle(
        GetAllFloorballTournamentsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball tournaments - Status filter: {Status}", request.Status ?? "All");

            IEnumerable<FloorballTournament> tournaments;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<FloorballTournamentStatus>(request.Status, ignoreCase: true, out FloorballTournamentStatus status))
                {
                    return Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>.Failure(
                        $"Invalid tournament status: '{request.Status}'. Valid values are: {string.Join(", ", Enum.GetNames<FloorballTournamentStatus>())}");
                }

                tournaments = await _tournamentRepository.GetByStatusAsync(status);
            }
            else
            {
                tournaments = await _tournamentRepository.GetAllAsync();
            }

            IReadOnlyCollection<FloorballTournamentSummaryDto> dtos = tournaments
                .Select(FloorballTournamentMapper.ToSummaryDto)
                .ToList()
                .AsReadOnly();

            _logger.LogInformation("Successfully retrieved {Count} floorball tournaments", dtos.Count);
            return Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball tournaments");
            return Result<IReadOnlyCollection<FloorballTournamentSummaryDto>>.Failure(
                "An error occurred while retrieving floorball tournaments.");
        }
    }
}
