using Application.Features.Floorball.Tournaments.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class DeleteFloorballTournamentHandler : IRequestHandler<DeleteFloorballTournamentCommand, Result>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballTournamentHandler> _logger;

    public DeleteFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool exists = await _tournamentRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball tournament with ID: {TournamentId}", request.Id);
                return Result.NotFound("FloorballTournament", request.Id);
            }

            _logger.LogInformation("Deleting floorball tournament with ID: {TournamentId}", request.Id);
            await _tournamentRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball tournament with ID: {TournamentId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball tournament: {TournamentId}", request.Id);
            return Result.Failure("An error occurred while deleting the floorball tournament.");
        }
    }
}
