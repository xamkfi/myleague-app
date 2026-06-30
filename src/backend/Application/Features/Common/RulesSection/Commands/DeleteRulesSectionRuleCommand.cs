using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for deleting a rule from a rules section
/// </summary>
public record DeleteRulesSectionRuleCommand(
    Guid SectionId,
    string RuleId,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for deleting a rule from a rules section
/// </summary>
public class DeleteRulesSectionRuleCommandHandler
    : IRequestHandler<DeleteRulesSectionRuleCommand, Result<RulesSectionDto>>
{
    private readonly IRulesSectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the DeleteRulesSectionRuleCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public DeleteRulesSectionRuleCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the DeleteRulesSectionRuleCommand request
    /// </summary>
    /// <param name="request">The command containing the section and rule IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        DeleteRulesSectionRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.SectionId,
            cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.SectionId}' not found.");
        }

        try
        {
            string updatedHtml = RulesHtmlHelper.DeleteRule(entity.ContentHtml, request.RuleId);
            entity.UpdateContentHtml(updatedHtml, request.LastModifiedBy);
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<RulesSectionDto>.Failure(ex.Message);
        }
    }
}
