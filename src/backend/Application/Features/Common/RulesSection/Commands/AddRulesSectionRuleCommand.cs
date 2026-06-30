using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for adding a rule to a rules section
/// </summary>
public record AddRulesSectionRuleCommand(
    Guid SectionId,
    string RuleHtml,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for adding a rule to a rules section
/// </summary>
public class AddRulesSectionRuleCommandHandler
    : IRequestHandler<AddRulesSectionRuleCommand, Result<RulesSectionDto>>
{
    private readonly IRulesSectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the AddRulesSectionRuleCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public AddRulesSectionRuleCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the AddRulesSectionRuleCommand request
    /// </summary>
    /// <param name="request">The command containing the rule HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        AddRulesSectionRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.SectionId,
            cancellationToken);

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

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
    }
}
