namespace Application.Features.Hockey.Seasons.DTOs;

public record HockeySeasonContentBlockDto(
    Guid Id,
    string Title,
    string ContentHtml,
    int SortOrder);

public record HockeySeasonContentBlocksDto(
    Guid? SeasonId,
    IReadOnlyList<HockeySeasonContentBlockDto> Blocks);
