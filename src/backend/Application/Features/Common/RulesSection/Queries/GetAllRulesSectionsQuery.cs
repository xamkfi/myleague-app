using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Queries;

/// <summary>
/// Query for retrieving all rules sections
/// </summary>
public record GetAllRulesSectionsQuery : IRequest<Result<IReadOnlyList<RulesSectionDto>>>;

/// <summary>
/// Handler for retrieving all rules sections
/// </summary>
public class GetAllRulesSectionsQueryHandler
    : IRequestHandler<GetAllRulesSectionsQuery, Result<IReadOnlyList<RulesSectionDto>>>
{
    private readonly IRulesSectionRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetAllRulesSectionsQueryHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    public GetAllRulesSectionsQueryHandler(IRulesSectionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the GetAllRulesSectionsQuery request
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All rules sections wrapped in a Result</returns>
    public async Task<Result<IReadOnlyList<RulesSectionDto>>> Handle(
        GetAllRulesSectionsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.Common.RulesSection> sections =
            await _repository.GetAllAsync(cancellationToken);

        IReadOnlyList<RulesSectionDto> dtos = sections
            .Select(RulesSectionMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<RulesSectionDto>>.Success(dtos);
    }
}
