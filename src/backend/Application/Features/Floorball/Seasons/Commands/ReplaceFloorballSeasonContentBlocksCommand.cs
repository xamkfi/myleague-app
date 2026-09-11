using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands;

public record ReplaceFloorballSeasonContentBlockItem(Guid? Id, string Title, string ContentHtml);

public record ReplaceFloorballSeasonContentBlocksCommand(
    Guid SeasonId,
    IReadOnlyList<ReplaceFloorballSeasonContentBlockItem> Items)
    : IRequest<Result<FloorballSeasonContentBlocksDto>>;
