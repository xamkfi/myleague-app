namespace Application.Features.Floorball.Seasons.DTOs;

public record FloorballSeasonContentBlockDto(
    Guid Id,
    string Title,
    string ContentHtml,
    int SortOrder);

public record FloorballSeasonContentBlocksDto(
    Guid? SeasonId,
    IReadOnlyList<FloorballSeasonContentBlockDto> Blocks);
