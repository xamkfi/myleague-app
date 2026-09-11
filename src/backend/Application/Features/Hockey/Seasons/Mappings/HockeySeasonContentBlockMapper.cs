using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Hockey.Competitions;

namespace Application.Features.Hockey.Seasons.Mappings;

public static class HockeySeasonContentBlockMapper
{
    public static HockeySeasonContentBlockDto ToDto(HockeySeasonContentBlock block) =>
        new(block.Id, block.Title, block.ContentHtml, block.SortOrder);

    public static HockeySeasonContentBlocksDto ToDtos(HockeySeason? season)
    {
        if (season is null)
        {
            return new HockeySeasonContentBlocksDto(null, Array.Empty<HockeySeasonContentBlockDto>());
        }

        IReadOnlyList<HockeySeasonContentBlockDto> blocks = season.ContentBlocks
            .OrderBy(block => block.SortOrder)
            .Select(ToDto)
            .ToList();

        return new HockeySeasonContentBlocksDto(season.Id, blocks);
    }
}
