using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Mappings;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Commands;

/// <summary>
/// Command for creating a season content block
/// </summary>
public record CreateSeasonContentBlockCommand(
    SportsCategory Sport,
    Guid CompetitionId,
    string SeasonYear,
    string Title,
    string ContentHtml,
    int SortOrder,
    string? LastModifiedBy
) : IRequest<Result<SeasonContentBlockDto>>;

/// <summary>
/// Handler for creating a season content block
/// </summary>
public class CreateSeasonContentBlockCommandHandler
    : IRequestHandler<CreateSeasonContentBlockCommand, Result<SeasonContentBlockDto>>
{
    private readonly ISeasonContentBlockRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the CreateSeasonContentBlockCommandHandler class
    /// </summary>
    public CreateSeasonContentBlockCommandHandler(
        ISeasonContentBlockRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<SeasonContentBlockDto>> Handle(
        CreateSeasonContentBlockCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.SeasonContentBlock entity = new(
            Guid.NewGuid(),
            request.Sport,
            request.CompetitionId,
            request.SeasonYear,
            request.Title,
            request.ContentHtml,
            request.SortOrder,
            request.LastModifiedBy);

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SeasonContentBlockDto>.Success(SeasonContentBlockMapper.ToDto(entity));
    }
}
