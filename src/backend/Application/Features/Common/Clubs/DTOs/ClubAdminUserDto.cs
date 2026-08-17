namespace Application.Features.Common.Clubs.DTOs;

/// <summary>
/// A user who administers a club.
/// </summary>
/// <param name="UserId">The user account ID</param>
/// <param name="PersonId">The person ID behind the user account</param>
/// <param name="FirstName">The person's first name</param>
/// <param name="LastName">The person's last name</param>
/// <param name="Email">The user's email address</param>
public record ClubAdminUserDto(
    Guid UserId,
    Guid PersonId,
    string FirstName,
    string LastName,
    string Email);
