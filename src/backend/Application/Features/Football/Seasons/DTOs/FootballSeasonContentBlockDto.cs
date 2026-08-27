namespace Application.Features.Football.Seasons.DTOs;

public record FootballSeasonContentBlockDto(
    Guid Id,
    string Title,
    string ContentHtml,
    int SortOrder);

public record FootballSeasonContentBlocksDto(
    Guid? SeasonId,
    IReadOnlyList<FootballSeasonContentBlockDto> Blocks);
