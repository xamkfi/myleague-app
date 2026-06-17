using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Commands;

public record UpdateRulesSectionRuleCommand(
    Guid SectionId,
    string RuleId,
    string RuleHtml,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

public class UpdateRulesSectionRuleCommandHandler
    : IRequestHandler<UpdateRulesSectionRuleCommand, Result<RulesSectionDto>>
{
    private readonly ICommonDbContext _context;

    public UpdateRulesSectionRuleCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RulesSectionDto>> Handle(
        UpdateRulesSectionRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _context.RulesSections
            .FirstOrDefaultAsync(x => x.Id == request.SectionId, cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.SectionId}' not found.");
        }

        try
        {
            string updatedHtml = RulesHtmlHelper.UpdateRule(
                entity.ContentHtml,
                request.RuleId,
                request.RuleHtml);

            entity.UpdateContentHtml(updatedHtml, request.LastModifiedBy);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<RulesSectionDto>.Failure(ex.Message);
        }
    }
}
