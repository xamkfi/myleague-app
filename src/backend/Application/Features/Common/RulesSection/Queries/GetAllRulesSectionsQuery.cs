using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Queries;

public record GetAllRulesSectionsQuery : IRequest<Result<IReadOnlyList<RulesSectionDto>>>;

public class GetAllRulesSectionsQueryHandler
    : IRequestHandler<GetAllRulesSectionsQuery, Result<IReadOnlyList<RulesSectionDto>>>
{
    private readonly ICommonDbContext _context;

    public GetAllRulesSectionsQueryHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<RulesSectionDto>>> Handle(
        GetAllRulesSectionsQuery request,
        CancellationToken cancellationToken)
    {
        List<Domain.Entities.Common.RulesSection> sections = await _context.RulesSections
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        IReadOnlyList<RulesSectionDto> dtos = sections
            .Select(RulesSectionMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<RulesSectionDto>>.Success(dtos);
    }
}
