using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using Domain.Enums.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Commands;

public record UpdateRulesSectionCommand(
    Guid Id,
    string Title,
    int SortOrder,
    RulesSectionType SectionType,
    Guid? ParentSectionId,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

public class UpdateRulesSectionCommandHandler
    : IRequestHandler<UpdateRulesSectionCommand, Result<RulesSectionDto>>
{
    private static readonly RulesSectionType[] MainTabSectionTypes =
    [
        RulesSectionType.Global,
        RulesSectionType.SportGroup,
        RulesSectionType.Validation,
        RulesSectionType.Fee,
    ];

    private readonly ICommonDbContext _context;

    public UpdateRulesSectionCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RulesSectionDto>> Handle(
        UpdateRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _context.RulesSections
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.Id}' not found.");
        }

        if (request.SectionType == RulesSectionType.Sport)
        {
            if (request.ParentSectionId == null)
            {
                return Result<RulesSectionDto>.Failure(
                    "Sport sections must belong under the Lajikohtaiset säännöt (SportGroup) section.");
            }

            Domain.Entities.Common.RulesSection? parent = await _context.RulesSections
                .FirstOrDefaultAsync(x => x.Id == request.ParentSectionId.Value, cancellationToken);

            if (parent == null)
            {
                return Result<RulesSectionDto>.Failure("Parent section not found.");
            }

            if (parent.SectionType != RulesSectionType.SportGroup)
            {
                return Result<RulesSectionDto>.Failure(
                    "Sport sections must belong under the Lajikohtaiset säännöt (SportGroup) section.");
            }
        }
        else if (MainTabSectionTypes.Contains(request.SectionType))
        {
            if (request.ParentSectionId != null)
            {
                return Result<RulesSectionDto>.Failure(
                    "Main tab sections cannot have a parent section.");
            }
        }
        else
        {
            return Result<RulesSectionDto>.Failure(
                $"Section type '{request.SectionType}' is not supported.");
        }

        entity.UpdateMetadata(
            request.Title,
            request.SortOrder,
            request.SectionType,
            request.ParentSectionId,
            request.LastModifiedBy);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
