using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Clubs.Handlers;

/// <summary>
/// Handler that replaces the set of club admins of a club: creates or reactivates club
/// manager links for the requested users and deactivates links for everyone else.
/// </summary>
public class SetClubAdminsHandler : IRequestHandler<SetClubAdminsCommand, Result<bool>>
{
    private readonly IClubRepository _clubRepository;
    private readonly IClubManagerRepository _clubManagerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetClubAdminsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the SetClubAdminsHandler class
    /// </summary>
    public SetClubAdminsHandler(
        IClubRepository clubRepository,
        IClubManagerRepository clubManagerRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetClubAdminsHandler> logger)
    {
        _clubRepository = clubRepository;
        _clubManagerRepository = clubManagerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SetClubAdminsCommand request
    /// </summary>
    public async Task<Result<bool>> Handle(SetClubAdminsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                return Result<bool>.Failure($"Club with ID '{request.ClubId}' was not found.");
            }

            // Resolve the requested users to person IDs
            HashSet<Guid> desiredPersonIds = new();
            foreach (Guid userId in request.UserIds.Distinct())
            {
                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<bool>.Failure($"User with ID '{userId}' was not found.");
                }

                desiredPersonIds.Add(user.PersonId);
            }

            IEnumerable<ClubManager> existingRows = await _clubManagerRepository.GetAllByClubIdAsync(request.ClubId);
            HashSet<Guid> handledPersonIds = new();

            foreach (ClubManager row in existingRows)
            {
                bool shouldBeActive = desiredPersonIds.Contains(row.PersonId);
                handledPersonIds.Add(row.PersonId);

                if (row.IsActive != shouldBeActive)
                {
                    row.UpdateActiveStatus(shouldBeActive);
                    await _clubManagerRepository.UpdateAsync(row);
                }
            }

            foreach (Guid personId in desiredPersonIds.Except(handledPersonIds))
            {
                await _clubManagerRepository.AddAsync(new ClubManager(personId, request.ClubId));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Club admins updated for club {ClubId}: {AdminCount} active admins",
                request.ClubId, desiredPersonIds.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting club admins for club {ClubId}", request.ClubId);
            return Result<bool>.Failure("An error occurred while updating the club admins.");
        }
    }
}
