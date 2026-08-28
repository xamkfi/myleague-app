using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Queries;

/// <summary>
/// Query for retrieving a rules section by ID
/// </summary>
public record GetRulesSectionByIdQuery(Guid Id) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for retrieving a rules section by ID
/// </summary>
public class GetRulesSectionByIdQueryHandler
    : IRequestHandler<GetRulesSectionByIdQuery, Result<RulesSectionDto>>
{
    private readonly IRulesSectionRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetRulesSectionByIdQueryHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    public GetRulesSectionByIdQueryHandler(IRulesSectionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the GetRulesSectionByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the section ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        GetRulesSectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.NotFound("RulesSection", request.Id);
        }

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
