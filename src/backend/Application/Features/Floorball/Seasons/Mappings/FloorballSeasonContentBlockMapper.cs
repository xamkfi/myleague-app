using Application.Features.Floorball.Seasons.DTOs;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;

namespace Application.Features.Floorball.Seasons.Mappings;

public static class FloorballSeasonContentBlockMapper
{
    public static FloorballSeasonContentBlockDto ToDto(FloorballSeasonContentBlock block) =>
        new(block.Id, block.Title, block.ContentHtml, block.SortOrder);

    public static FloorballSeasonContentBlocksDto ToDtos(FloorballSeason? season)
    {
        if (season is null)
        {
            return new FloorballSeasonContentBlocksDto(null, Array.Empty<FloorballSeasonContentBlockDto>());
        }

        IReadOnlyList<FloorballSeasonContentBlockDto> blocks = season.ContentBlocks
            .OrderBy(block => block.SortOrder)
            .Select(ToDto)
            .ToList();

        return new FloorballSeasonContentBlocksDto(season.Id, blocks);
    }
}
