using Application.Commands.Clubs;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for deleting a club
/// </summary>
public class DeleteClubHandler : IRequestHandler<DeleteClubCommand>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<DeleteClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteClubHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public DeleteClubHandler(IClubRepository clubRepository, ILogger<DeleteClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteClubCommand request
    /// </summary>
    /// <param name="request">The command containing the ID of the club to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when the club is not found</exception>
    public async Task Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.ClubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(request.ClubId));
        }

        bool exists = await _clubRepository.ExistsAsync(request.ClubId);
        if (!exists)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
            throw new InvalidOperationException($"Club with ID '{request.ClubId}' not found.");
        }

        _logger.LogInformation("Deleting club with ID: {ClubId}", request.ClubId);
        await _clubRepository.DeleteAsync(request.ClubId);
    }
} 