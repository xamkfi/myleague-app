namespace Application.Features.Common.Shared.DTOs;

/// <summary>
/// A person attached to a match (referee or scorekeeper) with a display name.
/// </summary>
/// <param name="Id">The referee/official ID for referees, or the person ID for scorekeepers</param>
/// <param name="Name">The person's full name, or an empty string when the person could not be resolved</param>
public record MatchPersonDto(Guid Id, string Name);
