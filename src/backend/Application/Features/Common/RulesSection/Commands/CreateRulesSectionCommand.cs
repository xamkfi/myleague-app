using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for creating a new rules section
/// </summary>
public record CreateRulesSectionCommand(
    string Title,
    int SortOrder,
    Domain.Enums.Common.RulesSectionType SectionType,
    Guid? ParentSectionId,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for creating a new rules section
/// </summary>
public class CreateRulesSectionCommandHandler
    : IRequestHandler<CreateRulesSectionCommand, Result<RulesSectionDto>>
{
    private static readonly Domain.Enums.Common.RulesSectionType[] MainTabSectionTypes =
    [
        Domain.Enums.Common.RulesSectionType.Global,
        Domain.Enums.Common.RulesSectionType.SportGroup,
        Domain.Enums.Common.RulesSectionType.Validation,
        Domain.Enums.Common.RulesSectionType.Fee,
    ];

    private readonly IRulesSectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the CreateRulesSectionCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public CreateRulesSectionCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the CreateRulesSectionCommand request
    /// </summary>
    /// <param name="request">The command containing section data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        CreateRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
        if (request.SectionType == Domain.Enums.Common.RulesSectionType.Sport)
        {
            if (request.ParentSectionId == null)
            {
                return Result<RulesSectionDto>.Failure(
                    "Sport sections must belong under the Lajikohtaiset säännöt (SportGroup) section.");
            }

            Domain.Entities.Common.RulesSection? parent = await _repository.GetByIdAsync(
                request.ParentSectionId.Value,
                cancellationToken);

            if (parent == null)
            {
                return Result<RulesSectionDto>.Failure("Parent section not found.");
            }

            if (parent.SectionType != Domain.Enums.Common.RulesSectionType.SportGroup)
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

            if (request.SectionType == Domain.Enums.Common.RulesSectionType.SportGroup)
            {
                bool sportGroupExists = await _repository.ExistsBySectionTypeAsync(
                    Domain.Enums.Common.RulesSectionType.SportGroup,
                    cancellationToken);

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

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
