using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

public record ReplaceHockeySeasonContentBlockItem(Guid? Id, string Title, string ContentHtml);

public record ReplaceHockeySeasonContentBlocksCommand(
    Guid SeasonId,
    IReadOnlyList<ReplaceHockeySeasonContentBlockItem> Items)
    : IRequest<Result<HockeySeasonContentBlocksDto>>;
