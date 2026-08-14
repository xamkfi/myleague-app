namespace Application.Features.Common.Users.DTOs;

/// <summary>
/// A team assignment included when inviting a team leader: which team (and sport)
/// the new team leader should manage.
/// </summary>
/// <param name="Sport">Sport discriminator: "floorball" or "football"</param>
/// <param name="TeamId">The team ID</param>
public record TeamAssignmentDto(string Sport, Guid TeamId);
