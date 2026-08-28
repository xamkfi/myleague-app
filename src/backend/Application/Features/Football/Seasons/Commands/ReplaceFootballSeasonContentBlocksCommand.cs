using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record ReplaceFootballSeasonContentBlockItem(Guid? Id, string Title, string ContentHtml);

public record ReplaceFootballSeasonContentBlocksCommand(
    Guid SeasonId,
    IReadOnlyList<ReplaceFootballSeasonContentBlockItem> Items)
    : IRequest<Result<FootballSeasonContentBlocksDto>>;
