namespace Application.Features.Common.Shared.DTOs;

/// <summary>
/// A team that has public all-time player statistics, for the team filter.
/// </summary>
public record AllTimeTeamOptionDto(Guid TeamId, string TeamName);
