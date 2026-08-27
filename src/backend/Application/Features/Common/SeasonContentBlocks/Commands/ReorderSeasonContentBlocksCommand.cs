using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Mappings;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Commands;

/// <summary>
/// Command for reordering season content blocks belonging to the same competition
/// </summary>
public record ReorderSeasonContentBlocksCommand(
    IReadOnlyList<Guid> OrderedIds,
    string? LastModifiedBy
) : IRequest<Result<IReadOnlyList<SeasonContentBlockDto>>>;

/// <summary>
/// Handler for reordering season content blocks
/// </summary>
public class ReorderSeasonContentBlocksCommandHandler
    : IRequestHandler<ReorderSeasonContentBlocksCommand, Result<IReadOnlyList<SeasonContentBlockDto>>>
{
    private readonly ISeasonContentBlockRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the ReorderSeasonContentBlocksCommandHandler class
    /// </summary>
    public ReorderSeasonContentBlocksCommandHandler(
        ISeasonContentBlockRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SeasonContentBlockDto>>> Handle(
        ReorderSeasonContentBlocksCommand request,
        CancellationToken cancellationToken)
    {
        List<Domain.Entities.Common.SeasonContentBlock> blocks = new(request.OrderedIds.Count);

        foreach (Guid id in request.OrderedIds)
        {
            Domain.Entities.Common.SeasonContentBlock? entity =
                await _repository.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return Result<IReadOnlyList<SeasonContentBlockDto>>.NotFound("SeasonContentBlock", id);
            }

            blocks.Add(entity);
        }

        Guid competitionId = blocks[0].CompetitionId;
        if (blocks.Any(block => block.CompetitionId != competitionId))
        {
            return Result<IReadOnlyList<SeasonContentBlockDto>>.Failure(
                "All content blocks in a reorder must belong to the same season.");
        }

        for (int index = 0; index < blocks.Count; index++)
        {
            blocks[index].SetSortOrder(index, request.LastModifiedBy);
            await _repository.UpdateAsync(blocks[index], cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        IReadOnlyList<SeasonContentBlockDto> dtos = blocks
            .Select(SeasonContentBlockMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<SeasonContentBlockDto>>.Success(dtos);
    }
}
