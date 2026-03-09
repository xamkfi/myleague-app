using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class ChangeFloorballTournamentStatusHandler : IRequestHandler<ChangeFloorballTournamentStatusCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeFloorballTournamentStatusHandler> _logger;

    public ChangeFloorballTournamentStatusHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<ChangeFloorballTournamentStatusHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(ChangeFloorballTournamentStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.Id);
            if (tournament is null)
            {
                _logger.LogWarning("Attempt to change status of non-existent floorball tournament with ID: {TournamentId}", request.Id);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.Id);
            }

            try
            {
                switch (request.Action.ToLowerInvariant())
                {
                    case "activate":
                        tournament.Activate();
                        break;
                    case "start":
                        tournament.Start();
                        break;
                    case "complete":
                        tournament.Complete();
                        break;
                    case "cancel":
                        tournament.Cancel();
                        break;
                    default:
                        return Result<FloorballTournamentDto>.Failure($"Invalid action: '{request.Action}'. Valid actions are: activate, start, complete, cancel.");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid status transition for tournament {TournamentId}: {Action}", request.Id, request.Action);
                return Result<FloorballTournamentDto>.Failure(ex.Message);
            }

            _logger.LogInformation("Changing status of floorball tournament {TournamentId} with action: {Action}", request.Id, request.Action);
            await _tournamentRepository.UpdateAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto dto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully changed status of floorball tournament {TournamentId} to {Status}", tournament.Id, tournament.Status);

            return Result<FloorballTournamentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing status of floorball tournament: {TournamentId}", request.Id);
            return Result<FloorballTournamentDto>.Failure("An error occurred while changing the tournament status.");
        }
    }
}
