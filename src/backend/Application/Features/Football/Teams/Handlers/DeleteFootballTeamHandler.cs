using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Football.Teams.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for deleting a football team that is not used in matches.
/// </summary>
public class DeleteFootballTeamHandler : IRequestHandler<DeleteFootballTeamCommand, Result>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<DeleteFootballTeamHandler> _logger;

    public DeleteFootballTeamHandler(
        IFootballTeamRepository teamRepository,
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<DeleteFootballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _matchRepository = matchRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool teamExists = await _teamRepository.ExistsAsync(request.Id);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to delete non-existent football team with ID: {TeamId}", request.Id);
                return Result.NotFound("FootballTeam", request.Id);
            }

            if (await _matchRepository.HasAnyForTeamAsync(request.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked football team delete for {TeamId}: team is used in matches", request.Id);
                return Result.Failure(DeletionReasons.TeamUsedInMatches);
            }

            _logger.LogInformation("Deleting football team with ID: {TeamId}", request.Id);
            await _teamRepository.DeleteAsync(request.Id);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football team with ID: {TeamId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football team: {TeamId}", request.Id);
            return Result.Failure("An error occurred while deleting the football team.");
        }
    }
}
