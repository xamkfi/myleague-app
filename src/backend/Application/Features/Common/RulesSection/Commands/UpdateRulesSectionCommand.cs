using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for updating an existing rules section
/// </summary>
public record UpdateRulesSectionCommand(
    Guid Id,
    string Title,
    int SortOrder,
    Domain.Enums.Common.RulesSectionType SectionType,
    Guid? ParentSectionId,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for updating an existing rules section
/// </summary>
public class UpdateRulesSectionCommandHandler
    : IRequestHandler<UpdateRulesSectionCommand, Result<RulesSectionDto>>
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
    /// Initializes a new instance of the UpdateRulesSectionCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public UpdateRulesSectionCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the UpdateRulesSectionCommand request
    /// </summary>
    /// <param name="request">The command containing updated section data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        UpdateRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.Id}' not found.");
        }

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

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
