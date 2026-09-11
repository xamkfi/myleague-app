using Application.Features.Football.Seasons.DTOs;
using Domain.Entities.Football.Competitions;

namespace Application.Features.Football.Seasons.Mappings;

public static class FootballSeasonContentBlockMapper
{
    public static FootballSeasonContentBlockDto ToDto(FootballSeasonContentBlock block) =>
        new(block.Id, block.Title, block.ContentHtml, block.SortOrder);

    public static FootballSeasonContentBlocksDto ToDtos(FootballSeason? season)
    {
        if (season is null)
        {
            return new FootballSeasonContentBlocksDto(null, Array.Empty<FootballSeasonContentBlockDto>());
        }

        IReadOnlyList<FootballSeasonContentBlockDto> blocks = season.ContentBlocks
            .OrderBy(block => block.SortOrder)
            .Select(ToDto)
            .ToList();

        return new FootballSeasonContentBlocksDto(season.Id, blocks);
    }
}
