using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.RulesSection.Commands;

public record CreateRulesSectionCommand(
    string Title,
    int SortOrder,
    RulesSectionType SectionType,
    Guid? ParentSectionId,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

public class CreateRulesSectionCommandHandler
    : IRequestHandler<CreateRulesSectionCommand, Result<RulesSectionDto>>
{
    private static readonly RulesSectionType[] MainTabSectionTypes =
    [
        RulesSectionType.Global,
        RulesSectionType.SportGroup,
        RulesSectionType.Validation,
        RulesSectionType.Fee,
    ];

    private readonly ICommonDbContext _context;

    public CreateRulesSectionCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RulesSectionDto>> Handle(
        CreateRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
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

            if (request.SectionType == RulesSectionType.SportGroup)
            {
                bool sportGroupExists = await _context.RulesSections
                    .AnyAsync(x => x.SectionType == RulesSectionType.SportGroup, cancellationToken);

                if (sportGroupExists)
                {
                    return Result<RulesSectionDto>.Failure(
                        "A SportGroup section already exists. Add sport-specific sections under it instead.");
                }
            }
        }
        else
        {
            return Result<RulesSectionDto>.Failure(
                $"Section type '{request.SectionType}' is not supported.");
        }

        Domain.Entities.Common.RulesSection entity = new(
            Guid.NewGuid(),
            request.Title,
            request.SortOrder,
            request.SectionType,
            request.ParentSectionId,
            string.Empty,
            request.LastModifiedBy);

        _context.RulesSections.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
