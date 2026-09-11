using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Persons.Commands;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Persons.Handlers;

/// <summary>
/// Handler for deleting a person after unused sport profiles are removed.
/// </summary>
public class DeletePersonHandler : IRequestHandler<DeletePersonCommand, Result>
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonDeletionGuard _personDeletionGuard;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFootballPlayerRepository _footballPlayerRepository;
    private readonly IHockeyPlayerRepository _hockeyPlayerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IFloorballRefereeRepository _floorballRefereeRepository;
    private readonly IFootballRefereeRepository _footballRefereeRepository;
    private readonly IHockeyOfficialRepository _hockeyOfficialRepository;
    private readonly IFloorballTeamManagerRepository _floorballTeamManagerRepository;
    private readonly IFootballTeamManagerRepository _footballTeamManagerRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly IHockeyUnitOfWork _hockeyUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePersonHandler> _logger;

    public DeletePersonHandler(
        IPersonRepository personRepository,
        IPersonDeletionGuard personDeletionGuard,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFootballPlayerRepository footballPlayerRepository,
        IHockeyPlayerRepository hockeyPlayerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IFloorballRefereeRepository floorballRefereeRepository,
        IFootballRefereeRepository footballRefereeRepository,
        IHockeyOfficialRepository hockeyOfficialRepository,
        IFloorballTeamManagerRepository floorballTeamManagerRepository,
        IFootballTeamManagerRepository footballTeamManagerRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        IHockeyUnitOfWork hockeyUnitOfWork,
        IUnitOfWork unitOfWork,
        ILogger<DeletePersonHandler> logger)
    {
        _personRepository = personRepository;
        _personDeletionGuard = personDeletionGuard;
        _floorballPlayerRepository = floorballPlayerRepository;
        _footballPlayerRepository = footballPlayerRepository;
        _hockeyPlayerRepository = hockeyPlayerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _floorballRefereeRepository = floorballRefereeRepository;
        _footballRefereeRepository = footballRefereeRepository;
        _hockeyOfficialRepository = hockeyOfficialRepository;
        _floorballTeamManagerRepository = floorballTeamManagerRepository;
        _footballTeamManagerRepository = footballTeamManagerRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
        _hockeyUnitOfWork = hockeyUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool personExists = await _personRepository.ExistsAsync(request.Id);
            if (!personExists)
            {
                _logger.LogWarning("Attempt to delete non-existent person with ID: {PersonId}", request.Id);
                return Result.NotFound("Person", request.Id);
            }

            PersonDeletionEvaluation evaluation =
                await _personDeletionGuard.EvaluateAsync(request.Id, cancellationToken);
            if (evaluation.IsBlocked)
            {
                _logger.LogWarning(
                    "Blocked person delete for {PersonId}: {Reason}",
                    request.Id,
                    evaluation.BlockReason);
                return Result.Failure(evaluation.BlockReason!);
            }

            if (evaluation.UnusedFloorballPlayerId.HasValue)
            {
                Guid playerId = evaluation.UnusedFloorballPlayerId.Value;
                IEnumerable<FloorballTeam> teams =
                    await _floorballTeamRepository.GetTeamsByPlayerIdAsync(playerId)
                    ?? Array.Empty<FloorballTeam>();
                foreach (FloorballTeam team in teams)
                {
                    team.RemovePlayer(playerId);
                }

                await _floorballPlayerRepository.DeleteAsync(playerId);
            }

            if (evaluation.UnusedFootballPlayerId.HasValue)
            {
                Guid playerId = evaluation.UnusedFootballPlayerId.Value;
                IEnumerable<FootballTeam> teams =
                    await _footballTeamRepository.GetTeamsByPlayerIdAsync(playerId)
                    ?? Array.Empty<FootballTeam>();
                foreach (FootballTeam team in teams)
                {
                    team.RemovePlayer(playerId);
                }

                await _footballPlayerRepository.DeleteAsync(playerId);
            }

            if (evaluation.UnusedHockeyPlayerId.HasValue)
            {
                await _hockeyPlayerRepository.DeleteUnusedProfileAsync(
                    evaluation.UnusedHockeyPlayerId.Value,
                    cancellationToken);
            }

            if (evaluation.UnusedFloorballRefereeId.HasValue)
            {
                await _floorballRefereeRepository.DeleteAsync(evaluation.UnusedFloorballRefereeId.Value);
            }

            if (evaluation.UnusedFootballRefereeId.HasValue)
            {
                await _footballRefereeRepository.DeleteAsync(evaluation.UnusedFootballRefereeId.Value);
            }

            if (evaluation.UnusedHockeyOfficialId.HasValue)
            {
                await _hockeyOfficialRepository.DeleteAsync(
                    evaluation.UnusedHockeyOfficialId.Value,
                    cancellationToken);
            }

            foreach (Guid managerId in evaluation.FloorballTeamManagerIds)
            {
                await _floorballTeamManagerRepository.DeleteAsync(managerId);
            }

            foreach (Guid managerId in evaluation.FootballTeamManagerIds)
            {
                await _footballTeamManagerRepository.DeleteAsync(managerId);
            }

            _logger.LogInformation("Deleting person with ID: {PersonId}", request.Id);
            await _personRepository.DeleteAsync(request.Id);

            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);
            await _hockeyUnitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted person with ID: {PersonId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting person: {PersonId}", request.Id);
            return Result.Failure("An error occurred while deleting the person.");
        }
    }
}
