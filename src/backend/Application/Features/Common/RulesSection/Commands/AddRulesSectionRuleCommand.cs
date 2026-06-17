using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Commands;

public record AddRulesSectionRuleCommand(
    Guid SectionId,
    string RuleHtml,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

public class AddRulesSectionRuleCommandHandler
    : IRequestHandler<AddRulesSectionRuleCommand, Result<RulesSectionDto>>
{
    private readonly ICommonDbContext _context;

    public AddRulesSectionRuleCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RulesSectionDto>> Handle(
        AddRulesSectionRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _context.RulesSections
            .FirstOrDefaultAsync(x => x.Id == request.SectionId, cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.SectionId}' not found.");
        }

        if (string.IsNullOrWhiteSpace(request.RuleHtml))
        {
            return Result<RulesSectionDto>.Failure("Rule HTML cannot be empty.");
        }

        string updatedHtml = RulesHtmlHelper.AppendRule(entity.ContentHtml, request.RuleHtml);
        entity.UpdateContentHtml(updatedHtml, request.LastModifiedBy);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
