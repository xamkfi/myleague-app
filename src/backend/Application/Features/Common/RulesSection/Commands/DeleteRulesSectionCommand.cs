using Application.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Commands;

public record DeleteRulesSectionCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteRulesSectionCommandHandler
    : IRequestHandler<DeleteRulesSectionCommand, Result<bool>>
{
    private readonly ICommonDbContext _context;

    public DeleteRulesSectionCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeleteRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _context.RulesSections
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"Rules section with ID '{request.Id}' not found.");
        }

        bool hasChildren = await _context.RulesSections
            .AnyAsync(x => x.ParentSectionId == request.Id, cancellationToken);

        if (hasChildren)
        {
            return Result<bool>.Failure("Cannot delete a section that has child sections.");
        }

        if (!string.IsNullOrWhiteSpace(entity.ContentHtml))
        {
            return Result<bool>.Failure("Cannot delete a section that contains rules. Remove all rules first.");
        }

        _context.RulesSections.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
