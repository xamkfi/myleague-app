using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Queries;

public record GetRulesSectionByIdQuery(Guid Id) : IRequest<Result<RulesSectionDto>>;

public class GetRulesSectionByIdQueryHandler
    : IRequestHandler<GetRulesSectionByIdQuery, Result<RulesSectionDto>>
{
    private readonly ICommonDbContext _context;

    public GetRulesSectionByIdQueryHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RulesSectionDto>> Handle(
        GetRulesSectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _context.RulesSections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.Id}' not found.");
        }

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
