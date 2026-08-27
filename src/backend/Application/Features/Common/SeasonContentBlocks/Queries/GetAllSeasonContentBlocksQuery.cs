using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Mappings;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Queries;

/// <summary>
/// Query for listing season content blocks by competition or by sport and season year
/// </summary>
public record GetAllSeasonContentBlocksQuery(
    Guid? CompetitionId,
    SportsCategory? Sport,
    string? SeasonYear
) : IRequest<Result<IReadOnlyList<SeasonContentBlockDto>>>;

/// <summary>
/// Handler for listing season content blocks
/// </summary>
public class GetAllSeasonContentBlocksQueryHandler
    : IRequestHandler<GetAllSeasonContentBlocksQuery, Result<IReadOnlyList<SeasonContentBlockDto>>>
{
    private readonly ISeasonContentBlockRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetAllSeasonContentBlocksQueryHandler class
    /// </summary>
    public GetAllSeasonContentBlocksQueryHandler(ISeasonContentBlockRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SeasonContentBlockDto>>> Handle(
        GetAllSeasonContentBlocksQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.Common.SeasonContentBlock> blocks;

        if (request.CompetitionId.HasValue && request.CompetitionId.Value != Guid.Empty)
        {
            blocks = await _repository.GetByCompetitionIdAsync(
                request.CompetitionId.Value,
                cancellationToken);
        }
        else
        {
            blocks = await _repository.GetBySportAndSeasonYearAsync(
                request.Sport!.Value,
                request.SeasonYear!,
                cancellationToken);
        }

        IReadOnlyList<SeasonContentBlockDto> dtos = blocks
            .Select(SeasonContentBlockMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<SeasonContentBlockDto>>.Success(dtos);
    }
}
