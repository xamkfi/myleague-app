using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Mappings;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Queries;

/// <summary>
/// Query for retrieving a season content block by ID
/// </summary>
public record GetSeasonContentBlockByIdQuery(Guid Id) : IRequest<Result<SeasonContentBlockDto>>;

/// <summary>
/// Handler for retrieving a season content block by ID
/// </summary>
public class GetSeasonContentBlockByIdQueryHandler
    : IRequestHandler<GetSeasonContentBlockByIdQuery, Result<SeasonContentBlockDto>>
{
    private readonly ISeasonContentBlockRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetSeasonContentBlockByIdQueryHandler class
    /// </summary>
    public GetSeasonContentBlockByIdQueryHandler(ISeasonContentBlockRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<Result<SeasonContentBlockDto>> Handle(
        GetSeasonContentBlockByIdQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.SeasonContentBlock? entity =
            await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<SeasonContentBlockDto>.NotFound("SeasonContentBlock", request.Id);
        }

        return Result<SeasonContentBlockDto>.Success(SeasonContentBlockMapper.ToDto(entity));
    }
}
