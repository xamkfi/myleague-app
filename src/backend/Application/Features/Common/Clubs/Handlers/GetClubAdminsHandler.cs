using Application.Common;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Clubs.Handlers;

/// <summary>
/// Handler that lists the users behind the active club manager links of a club.
/// </summary>
public class GetClubAdminsHandler : IRequestHandler<GetClubAdminsQuery, Result<IEnumerable<ClubAdminUserDto>>>
{
    private readonly IClubManagerRepository _clubManagerRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetClubAdminsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetClubAdminsHandler class
    /// </summary>
    public GetClubAdminsHandler(
        IClubManagerRepository clubManagerRepository,
        IUserRepository userRepository,
        ILogger<GetClubAdminsHandler> logger)
    {
        _clubManagerRepository = clubManagerRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetClubAdminsQuery request
    /// </summary>
    public async Task<Result<IEnumerable<ClubAdminUserDto>>> Handle(GetClubAdminsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<ClubAdminUserDto> admins = new();

            IEnumerable<ClubManager> managerRows = await _clubManagerRepository.GetAllByClubIdAsync(request.ClubId);
            foreach (ClubManager managerRow in managerRows.Where(m => m.IsActive))
            {
                User? user = await _userRepository.GetByPersonIdAsync(managerRow.PersonId);
                if (user == null)
                {
                    continue;
                }

                admins.Add(new ClubAdminUserDto(
                    user.Id,
                    managerRow.PersonId,
                    user.Person?.FirstName ?? string.Empty,
                    user.Person?.LastName ?? string.Empty,
                    user.Email));
            }

            return Result<IEnumerable<ClubAdminUserDto>>.Success(admins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club admins for club {ClubId}", request.ClubId);
            return Result<IEnumerable<ClubAdminUserDto>>.Failure("An error occurred while retrieving the club admins.");
        }
    }
}
